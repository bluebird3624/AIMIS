import React, { useEffect, useState } from 'react';
import * as icons from 'react-icons/io5';
import { absenceAPI, departmentAPI, usersAPI } from '../services/api';
import '../Styles/absence.css';

function Adminabsence() {
  const [requests, setRequests] = useState([]); // flat array of request objects
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [counts, setCounts] = useState({ pending: 0, approved: 0, rejected: 0, other: 0 });
  const newRequests = requests.filter(r => String(r.status || '').toLowerCase() === 'pending');
  const [users, setUsers] = useState({});

  const formatDate = (value) => {
    if (!value) return '-';
    try {
      const d = new Date(value);
      if (Number.isNaN(d.getTime())) return String(value);
      const y = d.getFullYear();
      const m = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      return `${y}-${m}-${day}`;
    } catch {
      return String(value);
    }
  };

  
  const getStatusClass = (status) => {
    const s = String(status || '').toLowerCase();
    if (s === 'pending') return 'absence-status-pending';
    if (s === 'approved') return 'absence-status-approved';
    if (s === 'rejected') return 'absence-status-rejected';
    return 'absence-status-pending'; 
  };

  
  const computeCounts = (list) => {
    const c = list.reduce((acc, r) => {
      const key = (r.status || 'other').toString().trim().toLowerCase();
      acc[key] = (acc[key] || 0) + 1;
      return acc;
    }, {});
    setCounts({
      pending: c.pending || 0,
      approved: c.approved || 0,
      rejected: c.rejected || 0,
      other: Object.keys(c).reduce((sum, k) => {
        if (!['pending', 'approved', 'rejected'].includes(k)) return sum + c[k];
        return sum;
      }, 0)
    });
  };

  
  const fetchRequests = async () => {
    setLoading(true);
    setError(null);
    try {
      
      const depRes = await departmentAPI.getDepartments();
      const departments = depRes?.data || [];

      const deptIds = departments.map(d => d.id).filter(Boolean);
      if (deptIds.length === 0) {
       
        const fallback = await absenceAPI.getAbsenceRequestsAll();
        const data = fallback?.data || [];
        setRequests(data);
        computeCounts(data);
        return;
      }

      
      const results = await Promise.allSettled(
        deptIds.map(id => absenceAPI.getRequestsByDepartment(id))
      );

      
      const allRequests = results.reduce((acc, r) => {
        if (r.status === 'fulfilled') {
          const respData = r.value?.data;
          if (Array.isArray(respData)) acc.push(...respData);
          else if (respData) acc.push(respData);
        } else {
        
          console.warn('department fetch failed', r.reason);
        }
        return acc;
      }, []);

      setRequests(allRequests);
      computeCounts(allRequests);
    } catch (err) {
      console.error('fetchRequests error', err);
      setError('Unable to load requests');
      setRequests([]);
      setCounts({ pending: 0, approved: 0, rejected: 0, other: 0 });
    } finally {
      setLoading(false);
    }
  };

  const fetchUsers = async () => {
    const result = await usersAPI.getUsers();
    const data = result.data;
    setUsers(data);
  }


  useEffect(() => {
    fetchRequests();
    fetchUsers();
    
  }, []);
  
  const usersMap = (id) => {
    const user = users.find(user => user.id === id);
    return user.userName;
  }
 
  const handleDecision = async (requestId, decision, note = '') => {
   
    const prev = requests;
    const updatedStatus = decision === 'approve' ? 'Approved' : 'Rejected';
    setRequests(prevList => prevList.map(r => (r.id === requestId ? { ...r, status: updatedStatus } : r)));
    computeCounts(requests.map(r => (r.id === requestId ? { ...r, status: updatedStatus } : r)));

    try {
      const payload = { decision: decision === 'approve' ? 'Approved' : 'Rejected', note };
      await absenceAPI.approveRejectAbsenceRequest(requestId, payload);
      
      await fetchRequests();
    } catch (err) {
      console.error('decision error', err);
      // rollback on error
      setRequests(prev);
      computeCounts(prev);
      setError('Failed to submit decision');
    }
  };

  return (
    <>
      <div className="greeting-message">
        <h1>Absence Management</h1>
      </div>

      <div className="stats-row" style={{ display: 'flex', gap: 12, alignItems: 'center', margin: '16px 0' }}>
        <div className="absence-stats-container">
          <div className="pending-icon-container">
            <icons.IoHourglassOutline style={{ fontSize: '40px' }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Pending</div>
            <div className="number">{counts.pending}</div>
          </div>
        </div>

        <div className="absence-stats-container">
          <div className="approved-icon-container">
            <icons.IoCheckmarkCircleOutline style={{ fontSize: '40px' }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Approved</div>
            <div className="number">{counts.approved}</div>
          </div>
        </div>

        <div className="absence-stats-container">
          <div className="rejected-icon-container">
            <icons.IoCloseCircleOutline style={{ fontSize: '40px' }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Rejected</div>
            <div className="number">{counts.rejected}</div>
          </div>
        </div>
      </div>

      <h2 style={{ fontFamily: 'arial', marginLeft: '20px' }}> Absence Requests</h2>

      <div className="absence-request-section">
        {loading && <div className="loading">Loading requests…</div>}
        {!loading && requests.length === 0 && <div className="empty">No requests</div>}

        

        {newRequests.map((r) => {
          const statusClass = getStatusClass(r.status);
          const displayStatus = r.status || 'Pending';
          return (
            <>
            <div key={r.id} className="request-container" style={{ marginBottom: 12 }}>
              <div className="top-row">
                <div className="request-user-info">
                  <div className="request-user-name">{ usersMap(r.userId) || 'Unknown'}</div>
                  <div className="absence-reason">{r.reason || '—'}</div>
                </div>

                <div className={statusClass} style={{ marginLeft: 'auto', textTransform: 'lowercase', textAlign: 'center' }}>
                  {String(displayStatus)}
                </div>
              </div>

              <div className="dates-row">
                <div className="date-container">
                  <div className="icon-container">
                    <icons.IoCalendarNumberOutline style={{ fontSize: '40px' }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="date-label">Start Date</div>
                    <div className="date">{formatDate(r.startDate)}</div>
                  </div>
                </div>

                <div className="date-container">
                  <div className="icon-container">
                    <icons.IoCalendarNumberOutline style={{ fontSize: '40px' }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="date-label">End Date</div>
                    <div className="date">{formatDate(r.endDate)}</div>
                  </div>
                </div>

                <div className="date-container">
                  <div className="icon-container">
                    <icons.IoTimerOutline style={{ fontSize: '40px' }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="date-label">Duration</div>
                    <div className="date">{r.duration || '—'}</div>
                  </div>
                </div>
              </div>
              <div className="actions">
               
                 
                  <p style={{ marginTop: 8 }}>{r.comments || 'no comment'}</p>

                  {/* show action buttons only for pending requests */}
                  {String(r.status || '').toLowerCase() === 'pending' && (
                    <div style={{ marginTop: 12, display: 'flex', gap: 12 }}>
                      <button
                        className="approve-button"
                        onClick={() => handleDecision(r.id, 'approve')}
                        type="button"
                      >
                        <icons.IoCheckmarkCircleOutline />
                        Approve
                      </button>

                      <button
                        className="reject-button"
                        onClick={() => handleDecision(r.id, 'reject')}
                        type="button"
                      >
                        <icons.IoCloseCircleOutline />
                        Reject
                      </button>
                    </div>
                  )}
              
              </div>
          
      </div>
      </>
    );
    
        })}

      <h2 style={{ fontFamily: 'arial', marginLeft: '20px' }}> Recent Decisions</h2>

      <div className="recent-decision-section">
      
        {requests
          .filter(r => {
            const s = String(r.status || '').toLowerCase();
            return s === 'approved' || s === 'rejected';
          })
          .slice(0, 4)
          .map((r) => (
            <div key={`recent-${r.id}`} className="recent-decision-container">
              <div className="top-row">
                <div className="request-user-info">
                  <div className="request-user-name">{usersMap(r.userId) || 'Unknown'}</div>
                  <div className="absence-reason">{r.reason || '—'}</div>
                </div>
                <div className={getStatusClass(r.status)} style={{ marginLeft: 'auto', textAlign: 'center' }}>
                  {r.status}
                </div>
              </div>

              <div className="dates-row">
                <div className="date-container">
                  <div className="icon-container">
                    <icons.IoCalendarNumberOutline style={{ fontSize: '40px' }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="date-label">Start Date</div>
                    <div className="date">{formatDate(r.startDate)}</div>
                  </div>
                </div>

                <div className="date-container">
                  <div className="icon-container">
                    <icons.IoCalendarNumberOutline style={{ fontSize: '40px' }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="date-label">End Date</div>
                    <div className="date">{formatDate(r.endDate)}</div>
                  </div>
                </div>

                <div className="date-container">
                  <div className="icon-container">
                    <icons.IoTimerOutline style={{ fontSize: '40px' }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="date-label">Duration</div>
                    <div className="date">{r.duration || '—'}</div>
                  </div>
                </div>
              </div>
            </div>
          ))}
      </div>
      </div>
      </>
   
  );
}

export default Adminabsence;
