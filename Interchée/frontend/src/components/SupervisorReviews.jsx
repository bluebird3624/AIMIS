import { useState, useRef, useEffect } from "react";
import '../styles/reviews.css';
import * as icons from 'react-icons/io5';
import { ReviewAPI, usersAPI } from "../services/api";
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { TextField } from '@mui/material';
import {getCurrentUser} from '../services/auth'




export default function SupervisorReviews() {
    const [viewMore, setViewMore] = useState(false);
    const [isScheduleModalOpen, setIsScheduleModalOpen] = useState(false);
    const [scheduledReviews, setScheduledReviews] = useState([]);
    const [reviewData, setReviewData] = useState({
        title: '',
        description: '',
        userIds: [''],
        location: '',
        scheduledAt: '',
    });
    const [studentsList, setStudentsList] = useState([]);
    const [editingReviewId, setEditingReviewId] = useState(null); // Track which review is being edited

    const user = getCurrentUser();
    const userId = user.userId;
    
    const fetchReviews  = async () => {
        const response = await ReviewAPI.fetchReviews(userId);
        setScheduledReviews(response.data);
    }

    

    const fetchUsers = async() => {
        const response = await usersAPI.getUsers();
        setStudentsList(response.data);

    }

    useEffect(() => {
        fetchReviews();
        fetchUsers();
        
    }, []);

    const handleStudentToggle = (student) => {
        setReviewData(prev => ({
            ...prev,
            userIds: prev.userIds.includes(student)
                ? prev.userIds.filter(s => s !== student)
                : [...prev.userIds, student]
        }));
    };

        //function needs modification to set the student ids and not the student object itself
    const handleSelectAllStudents = () => {
        setReviewData(prev => ({
            ...prev,
            students: prev.students.length === studentsList.length ? [] : [...studentsList]
        }));
    };

    const handleInputChange = (field, value) => {
        setReviewData(prev => ({
            ...prev,
            [field]: value
        }));
    };

    const handleSubmitReview = async() => {
        if (editingReviewId) {
           
            setScheduledReviews(prev => 
                prev.map(review => 
                    review.id === editingReviewId 
                        ? {
                            ...review,
                            title: reviewData.title,
                            description: reviewData.description,
                            userIds: [...reviewData.userIds],
                            location: reviewData.location,
                        }
                        : review
                )
            );
        } else {
            
           

            
            console.log("formatted review data: ",reviewData)

            await ReviewAPI.createReview(reviewData)
            setScheduledReviews(prev => [reviewData, ...prev]);
            
        }
        
       
        handleCloseModal();
    };

   
    const handleReschedule = (review) => {
        
        setReviewData({
            title: review.title,
            description: review.description,
            students: [...review.students],
            location: review.location,
            scheduledAt: review.time,
            
        });
        
      
        setEditingReviewId(review.id);
        
        // Open the modal
        setIsScheduleModalOpen(true);
    };

    // Function to close modal and reset form
    const handleCloseModal = () => {
        setIsScheduleModalOpen(false);
        setEditingReviewId(null);
        setReviewData({
            title: '',
            description: '',
            userIds: [],
            location: '',
            scheduledAt: ''
           
        });
    };

    // Function to handle creating a new review (when not editing)
    const handleNewReview = () => {
        setEditingReviewId(null);
        setReviewData({
            title: '',
            description: '',
            userIds: [],
            location: '',
            scheduledAt: '',
            
        });
        setIsScheduleModalOpen(true);
    };

    const isFormValid = reviewData.title && reviewData.description && 
                       reviewData.userIds.length > 0 && reviewData.location && 
                       reviewData.scheduledAt;

    // Format date to display in a more readable format
    const formatDate = (dateString) => {
        const options = { year: 'numeric', month: 'short', day: 'numeric' };
        return new Date(dateString).toLocaleDateString(undefined, options);
    };

    // Format date for date input (YYYY-MM-DD)
    const formatDateForInput = (dateString) => {
        return new Date(dateString).toISOString().split('T')[0];
    };

    return (
        <>
            <div className="header-row">
                <h1 style={{ fontFamily:"arial", fontSize: "35px", marginLeft: "20px"}}> Reviews </h1>
                <button 
                    className="schedule-rev-btn"
                    onClick={handleNewReview}
                >
                    <icons.IoAdd/> Schedule review
                </button>
            </div>
            <p style={{ fontFamily: 'arial', fontSize:'20px', marginLeft:'20px', color: '#3d3d3d'}}> Schedule review sessions with your students</p>
            <div className="rev-category-row">
                <button>Upcoming</button>
                <button>Completed</button>
            </div>

            <div className="rev-cards-section">
                {scheduledReviews.length === 0 ? (
                    <div className="rev-empty-state">
                        <h3>No Reviews Scheduled</h3>
                    </div>
                ) : (
                    scheduledReviews.map(review => (
                        <div key={review.id} className="review-card">
                            <div className="review-card-toprow">
                                <p className="card-revtitle">{review.title}</p>
                                <span className="rev-status-badge">{review.status}</span>
                            </div>
                            
                            <div className="card-statsrow">
                                <div className="rev-date-container">
                                    <div className="rev-icon-container">
                                        <icons.IoCalendarNumberOutline/>
                                    </div>
                                    <div className="rev-content-wrapper">
                                        <div className="rev-date-label">Date</div>
                                        <div className="rev-date">{formatDate(review.date)}</div>
                                    </div>
                                </div>

                                <div className="rev-date-container">
                                    <div className="rev-icon-container">
                                        <icons.IoTimeOutline/>
                                    </div>
                                    <div className="rev-content-wrapper">
                                        <div className="rev-date-label">Time</div>
                                        <div className="rev-date">{review.time}</div>
                                    </div>
                                </div>
                            </div>
                            
                            <div className="card-statsrow">
                                <div className="rev-date-container">
                                    <div className="rev-icon-container">
                                        <icons.IoLocationOutline/>
                                    </div>
                                    <div className="rev-content-wrapper">
                                        <div className="rev-date-label">Location</div>
                                        <div className="rev-date">{review.location}</div>
                                    </div>
                                </div>

                                <div className="rev-date-container">
                                    <div className="rev-icon-container">
                                        <icons.IoPeopleOutline/>
                                    </div>
                                    <div className="rev-content-wrapper">
                                        <div className="rev-date-label">Participants</div>
                                        <div className="rev-date">{review.userIds.length}</div>
                                    </div>
                                </div>
                            </div>
                            {review.description && (
                                <div className="rev-description">
                                    <h2> Review Description</h2>
                                    <p>{review.description}</p>
                                </div>
                            )}
                            
                         
                            
                            <div className="rev-card-actions">
                                <button 
                                    className="rev-complete-btn"
                                    onClick={() => handleReschedule(review)}
                                >
                                    <icons.IoRefreshOutline/> Reschedule
                                </button>
                            </div>
                        </div>
                    ))
                )}
            </div>

            {/* REVIEW MODAL */}
            {isScheduleModalOpen && (
                <div className="rev-modal-overlay">
                    <div className="rev-modal-container">
                        <div className="rev-modal-header">
                            <h2 className="rev-modal-title">
                                {editingReviewId ? 'Reschedule Review' : 'Schedule New Review'}
                            </h2>
                            <button 
                                className="rev-modal-close"
                                onClick={handleCloseModal}
                            >
                                <icons.IoClose />
                            </button>
                        </div>
                        
                        <div className="rev-modal-content">
                            <div className="rev-form-group">
                                <label className="rev-form-label">Review Title *</label>
                                <input
                                    type="text"
                                    className="rev-form-input"
                                    placeholder="Enter review title"
                                    value={reviewData.title}
                                    onChange={(e) => handleInputChange('title', e.target.value)}
                                />
                            </div>
                            <div className="rev-form-group">
                                <label className="rev-form-label">Review Description</label>
                                <textarea
                                    className="rev-form-textarea"
                                    placeholder="Enter review description"
                                    rows="4"
                                    value={reviewData.description}
                                    onChange={(e) => handleInputChange('description', e.target.value)}
                                />
                            </div>
                            <div className="rev-form-group">
                                <label className="rev-form-label">Students Involved *</label>
                                <div className="rev-students-container">
                                    <button 
                                        type="button"
                                        className="rev-select-all-btn"
                                        onClick={handleSelectAllStudents}
                                    >
                                        {reviewData.userIds.length === studentsList.length ? 'Deselect All' : 'Select All'}
                                    </button>
                                    <div className="rev-students-list">
                                        {studentsList.map((student, index) => (
                                            <div key={index} className="rev-student-option">
                                                <input
                                                    key={index}
                                                    type="checkbox"
                                                    id={`student-${index}`}
                                                    checked={reviewData.userIds.includes(student.id)}
                                                    onChange={() => handleStudentToggle(student.id)}
                                                    className="rev-student-checkbox"
                                                />
                                                <label 
                                                    htmlFor={`student-${index}`}
                                                    className="rev-student-label"
                                                >
                                                    {student.userName}
                                                </label>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                                <div className="rev-selected-students">
                                    <strong>Selected ({reviewData.userIds.length}): </strong>
                                    {reviewData.userIds.length > 0 ? reviewData.userIds.join(', ') : 'None'}
                                </div>
                            </div>
                            <div className="rev-form-group">
                                <label className="rev-form-label">Location *</label>
                                <input
                                    type="text"
                                    className="rev-form-input"
                                    placeholder="Enter meeting location"
                                    value={reviewData.location}
                                    onChange={(e) => handleInputChange('location', e.target.value)}
                                />
                            </div>
                            <div className="rev-datetime-row">
                                <LocalizationProvider dateAdapter={AdapterDateFns}>
                                    <div className="rev-form-group">
                                        <label className="rev-form-label">Scheduled Date & Time *</label>
                                        <DateTimePicker
                                            value={reviewData.scheduledAt}
                                            onChange={(newValue) => handleInputChange('scheduledAt', newValue)}
                                            renderInput={(params) => (
                                                <TextField 
                                                    {...params} 
                                                    fullWidth
                                                    className="rev-form-input"
                                                />
                                            )}
                                            minDateTime={new Date()} // Prevent past dates
                                        />
                                    </div>
                                </LocalizationProvider>
                                
                                {/* <div className="rev-form-group rev-time-group">
                                    <label className="rev-form-label">Scheduled At</label>
                                    
                                    {/* <select
                                        className="rev-form-select"
                                        value={reviewData.scheduledAt}
                                        onChange={(e) => handleInputChange('scheduledAt', e.target.value)}
                                    >
                                        <option value="">Select time</option>
                                        {timeSlots.map((time, index) => (
                                            <option key={index} value={time}>
                                                {time}
                                            </option>
                                        ))}
                                    </select> 
                                </div> */}
                            </div>
                        </div>

                        <div className="rev-modal-actions">
                            <button 
                                className="rev-cancel-btn"
                                onClick={handleCloseModal}
                            >
                                Cancel
                            </button>
                            <button 
                                className="rev-submit-btn"
                                onClick={handleSubmitReview}
                                disabled={!isFormValid}
                            >
                                {editingReviewId ? 'Update Review' : 'Schedule Review'}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}