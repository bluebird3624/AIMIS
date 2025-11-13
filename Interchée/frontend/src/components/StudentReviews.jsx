import { useState, useEffect } from "react";
import * as icons from 'react-icons/io5';
import '../styles/reviews.css'; 
import { ReviewAPI } from "../services/api";
import { getCurrentUser } from "../services/auth";

export default function StudentReviews() {
    
    const [reviews, setReviews] = useState([]);

    const fetchReviews = async () => {
        const user = getCurrentUser();
        const response = await ReviewAPI.fetchReviews(user.userId);
        console.log(response);
        setReviews(response.data);

    }

    useEffect(()=> {
        fetchReviews();
    },[]);



    return (
        <>
        <h1 style={{ fontFamily:"arial", fontSize: "35px", marginLeft: "20px"}}> Reviews s</h1>
        <div className="rev-category-row">
                <button>Upcoming</button>
                <button>Completed</button>
        </div>
        <div className="rev-cards-section">
                                {reviews.map((review, index) => (
                                    <div className="review-card"
                                        key={index}
                                    >
                                        <div className="review-card-toprow">
                                            <p className="card-revtitle">-</p>
                                            <span className="rev-status-badge">-</span>
                                        </div>
                                        
                                        <div className="card-statsrow">
                                            <div className="rev-date-container">
                                                <div className="rev-icon-container">
                                                    <icons.IoCalendarNumberOutline/>
                                                </div>
                                                <div className="rev-content-wrapper">
                                                    <div className="rev-date-label">Sheduled AT</div>
                                                    <div className="rev-date"> {review.scheduledAt} </div>
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
                                                    <icons.IoPersonOutline/>
                                                </div>
                                                <div className="rev-content-wrapper">
                                                    <div className="rev-date-label">Supervisor</div>
                                                    <div className="rev-date">{review.supervisorId}</div>
                                                </div>
                                            </div>
                                        </div>
                                            <div className="rev-description">
                                                <h2> Review Description</h2>
                                                <p>{review.description}</p>
                                            </div>
                                                                                
                                        <div className="involved-students-row">
                                            <strong>Involved Students: </strong> 
                                        </div>
                                        <div className="students-row">
                                                <div className="student-name-container">
                                                    
                                                </div>
                                            
                                        </div>
                                        
                                        
                                    </div> 
                                    ))}
                                
                        
                        
        </div>

        </>

    )
    
}