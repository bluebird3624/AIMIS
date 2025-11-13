import React, { useEffect, useState } from 'react';
import {absenceAPI} from '../services/api'; 
import '../Styles/Absence.css';
import  * as icons from 'react-icons/io5';

const StudentAbsence = () => {
  const [requests, setRequests] = useState([]);
  const [absenceModal, setAbsenceModal] = useState(false);
  const [summary, setSummary] = useState({ pending: 0, approved: 0, rejected: 0 });
  const [form, setForm] = useState({
    startDate: '',
    endDate: '',
    reason: ''
  });
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  
  const fetchRequests = async () => {
    setLoading(true);
    setError(null);
    try {
     
      const res = await absenceAPI.getMyAbsenceRequests(); 
      const data = res?.data || [];
      setRequests(data);

      const counts = data.reduce((acc, r) => {
        const key =  r.status;
        console.log('key being used: ', key);
        acc[key] = (acc[key] || 0) + 1;
        console.log('summary: ', acc);
        return acc;
      }, {});
      setSummary({
        pending: counts.Pending || 0,
        approved: counts.Approved || 0,
        rejected: counts.Rejected || 0
      });
    } catch (err) {
      console.error('Failed to fetch requests', err);
      setError('Unable to load requests');
      setRequests([]);
      setSummary({ pending: 0, approved: 0, rejected: 0 });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRequests();
  }, []);

  const formatDateYMD = (value) => {
  if (!value) return null;
  // already yyyy-mm-dd
  if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return value;
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return null;
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
};

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm(f => ({ ...f, [name]: value }));
  };


  const submitRequest = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const payload = {
        startDate: form.startDate,
        endDate: form.endDate,
        reason: form.reason,
        // comments: form.comments
      };
      
      const res = await absenceAPI.creatAbsenceRequest(payload);
      
      const created = res?.data;
      if (created) {
        setRequests(prev => [created, ...prev]);
      } else {
       
        await fetchRequests();
      }
      setForm({ startDate: '', endDate: '', reason: '' });
    } catch (err) {
      console.error('Submit failed', err);
      setError('Start date cannot be  in the past.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleCreateRequest = () => {
    setAbsenceModal(true);
    
  }

  const closeModal = () => {
    setAbsenceModal(false);
    setForm({ startDate: '', endDate: '', reason: '' });
    setError(null);
  }

  // Filter requests by status
  const pendingRequests = requests.filter(request => request.status === 'Pending');
  const pastRequests = requests.filter(request => request.status !== 'Pending');

const renderRequestCard = (r) => (
  <div key={r.id} className="request-container">
    <div className="top-row">
      <div className="request-user-info">
        <div className="request-user-name">{r.requestedByName || r.userName || 'You'}</div>
        <div className="absence-reason">{r.reason}</div>
      </div>
      <div 
        className={`absence-status ${
          r.status.toLowerCase() === 'pending' ? 'absence-status-pending' :
          r.status.toLowerCase() === 'approved' ? 'absence-status-approved' :
          'absence-status-rejected'
        }`}
        style={{ marginLeft: 'auto' }}
      >
        {r.status}
      </div>
    </div>

    <div className="dates-row">
      <div className="date-container">
        <div className="icon-container">
            <icons.IoCalendarNumberOutline style={{ fontSize: '40px' }} />
        </div>
        <div className='ab-content-wrapper'>
        <div className="date-label">From</div>
        <div className="date">{r.startDate?.slice(0,10) || '-'}</div>
        </div>
      </div>

      <div className="date-container">
        <div className="icon-container">
            <icons.IoCalendarNumberOutline style={{ fontSize: '40px' }} />
        </div>
        <div className='ab-content-wrapper'>
        <div className="date-label">To</div>
        <div className="date">{r.endDate?.slice(0,10) || '-'}</div>
        </div>
      </div>

      <div className="date-container">
        <div className="icon-container">
            <icons.IoTimerOutline style={{ fontSize: '40px' }} />
        </div>
        <div className='ab-content-wrapper'>
        <div className="date-label">Duration</div>
         <div className="date">{r.days != null ? `${r.days} day(s)` : '-'}</div>
        </div>
      </div>
    </div>
    </div>

);

  return (
    <>
    <div className='title'>
            <h1 style={{ fontFamily:"arial", fontSize: " 35px"}}> Absence</h1>
     </div>   
       <p style={{ fontFamily: 'arial', fontSize:'20px', marginLeft:'20px', color: '#3d3d3d'}}>Request and track your leave requests</p>

          
        <button className="request-button" onClick={handleCreateRequest} type="button"> < icons.IoWalkOutline/>Request Absence</button> 

                {/** stats section */}
                      <div className="stats-row" style={{ display: 'flex', gap: 12, alignItems: 'center', margin: '16px 0' }}>
                        <div className="absence-stats-container">
                          <div className="pending-icon-container">
                            <icons.IoHourglassOutline style={{ fontSize: '40px' }} />
                          </div>
                          <div className="content-wrapper">
                            <div className="label">Pending</div>
                            <div className="number">{summary.pending}</div>
                          </div>
                        </div>
                
                        <div className="absence-stats-container">
                          <div className="approved-icon-container">
                            <icons.IoCheckmarkCircleOutline style={{ fontSize: '40px' }} />
                          </div>
                          <div className="content-wrapper">
                            <div className="label">Approved</div>
                            <div className="number">{summary.approved}</div>
                          </div>
                        </div>
                
                        <div className="absence-stats-container">
                          <div className="rejected-icon-container">
                            <icons.IoCloseCircleOutline style={{ fontSize: '40px' }} />
                          </div>
                          <div className="content-wrapper">
                            <div className="label">Rejected</div>
                            <div className="number">{summary.rejected}</div>
                          </div>
                        </div>
                      </div>  

              <h1 style={{ fontFamily: 'arial', fontSize: '30px', marginLeft:'20px'}}> Pending requests</h1>
              <div className='section'>
                <div className='requests-container'>
                  {loading && <div className="loading">Loading requests…</div>}
                  {!loading && pendingRequests.length === 0 && <div className="no-requests">No pending requests</div>}
                  {pendingRequests.map(renderRequestCard)}
                </div>
              </div>

              <h1 style={{ fontFamily: 'arial', fontSize: '30px', marginLeft:'20px'}}> Past requests</h1>
              <div className='section'>
                <div className='requests-container'>
                  {loading && <div className="loading">Loading requests…</div>}
                  {!loading && pastRequests.length === 0 && <div className="no-requests">No past requests</div>}
                  {pastRequests.map(renderRequestCard)}
                </div>
              </div>     

      {/* modal for request form */}
      {absenceModal && (
        <div className="absence-overlay" onClick={closeModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>New Absence Request</h3>
            </div>

            <form className="absence-form" onSubmit={submitRequest}>
              <div className="absence-form-row">
                <label>Start date</label>
                <input name="startDate" type="date" value={form.startDate} onChange={handleChange} required />
              </div>

              <div className="absence-form-row">
                <label>End date</label>
                <input name="endDate" type="date" value={form.endDate} onChange={handleChange} required />
              </div>

              <div className="absence-form-row">
                <label>Reason</label>
                <input name="reason" type="text" value={form.reason} onChange={handleChange} placeholder="e.g. Medical" required />
              </div>

              

              <div className="absence-form-actions">
                <button type="submit" className="btn-primary" disabled={submitting}>
                  {submitting ? 'Sending...' : 'Send Request'}
                </button>
                <button type="button" className="btn-cancel" onClick={closeModal}>Cancel</button>
              </div>

              {error && <div className="form-error">{error}</div>}
            </form>
          </div>
        </div>
      )}

          
    </>
  );
};

export default StudentAbsence;