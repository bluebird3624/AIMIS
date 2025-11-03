
import React, { useEffect, useState } from 'react';
import {absenceAPI} from '../services/api'; 
import '../Styles/absence.css';

const StudentAbsence = () => {
  const [requests, setRequests] = useState([]);
  const [absenceModal, setAbsenceModal] = useState(false);
  const [summary, setSummary] = useState({ pending: 0, approved: 0, rejected: 0 });
  const [form, setForm] = useState({
    startDate: '',
    endDate: '',
    reason: '',
    comments: ''
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
      setForm({ startDate: '', endDate: '', reason: '', comments: '' });
    } catch (err) {
      console.error('Submit failed', err);
      setError('Failed to submit request');
    } finally {
      setSubmitting(false);
    }
  };

  const handleCreateRequest = () => {
    setAbsenceModal(true);
    
  }

  const closeModal = () => {
    setAbsenceModal(false);
    setForm({ startDate: '', endDate: '', reason: '', comments: '' });
    setError(null);
  }

  return (
    <div className='student-absence-main-content'>
      <div className='request-absence-header'>
        <div className="header-row">
          <div className="header-left">
            <h1>Absence Management</h1>
            <p>Request and manage leave requests</p>
          </div>

          <div className="header-right">
            <button className="btn-primary" onClick={handleCreateRequest} type="button">Request Absence</button>
          </div>
        </div>

        <div className='request-absence-heading'>
          {/* summary remains here */}
        </div>
      </div>

      <div className='request-absence-summary'>
        <div className="absence-stats">
          <div className="absence-stats-container">
            <div>
              <div className="pending-icon-container"><svg width="18" height="18" /></div>
            </div>
            <div>
              <div className="stat-label">Pending</div>
              <div className="stat-value">{summary.pending}</div>
            </div>
          </div>

          <div className="absence-stats-container">
            <div>
              <div className="approved-icon-container"><svg width="18" height="18" /></div>
            </div>
            <div>
              <div className="stat-label">Approved</div>
              <div className="stat-value">{summary.approved}</div>
            </div>
          </div>

          <div className="absence-stats-container">
            <div>
              <div className="rejected-icon-container"><svg width="18" height="18" /></div>
            </div>
            <div>
              <div className="stat-label">Rejected</div>
              <div className="stat-value">{summary.rejected}</div>
            </div>
          </div>
        </div>
      </div>

      {/* modal for request form */}
      {absenceModal && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>New Absence Request</h3>
              <button className="modal-close" onClick={closeModal} aria-label="Close" type="button">×</button>
            </div>

            <form className="absence-form" onSubmit={submitRequest}>
              <div className="form-row">
                <label>Start</label>
                <input name="startDate" type="date" value={form.startDate} onChange={handleChange} required />
              </div>

              <div className="form-row">
                <label>End</label>
                <input name="endDate" type="date" value={form.endDate} onChange={handleChange} required />
              </div>

              <div className="form-row">
                <label>Reason</label>
                <input name="reason" type="text" value={form.reason} onChange={handleChange} placeholder="e.g. Medical" required />
              </div>

              <div className="form-row">
                <label>Comments</label>
                <textarea name="comments" value={form.comments} onChange={handleChange} placeholder="Optional details" />
              </div>

              <div className="form-actions">
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

      <div className='request-absence-body'>
        <div className='requests-container'>
          {loading && <div className="loading">Loading requests…</div>}
          {!loading && requests.length === 0 && <div className="empty">No requests yet</div>}

          {requests.map((r) => (
            <div key={r.id} className="request-container">
              <div className="top-row">
                <div className="request-user-info">
                  <div className="request-user-name">{r.requestedByName || r.userName || 'You'}</div>
                  <div className="absence-reason">{r.reason}</div>
                </div>
                <div className={`absence-status-${r.status}`} style={{ marginLeft: 'auto' }}>
                  {r.status}
                </div>
              </div>

              <div className="dates-row">
                <div className="date-container">
                  <div className="date-label">From</div>
                  <div className="date">{r.startDate?.slice(0,10) || '-'}</div>
                </div>

                <div className="date-container">
                  <div className="date-label">To</div>
                  <div className="date">{r.endDate?.slice(0,10) || '-'}</div>
                </div>
              </div>

              <div className="recent-decision-container">
                <div className="comment-block" style={{ padding: 12 }}>
                  <strong>Comments</strong>
                  <p style={{ marginTop: 8 }}>{r.comments || '—'}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default StudentAbsence;