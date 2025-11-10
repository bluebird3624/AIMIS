import { useState } from "react";
import * as icons from 'react-icons/io5';
import '../styles/reviews.css'; 


export default function StudentReviews() {
    



    return (
        <>
        <h1 style={{ fontFamily:"arial", fontSize: "35px", marginLeft: "20px"}}> Reviews s</h1>
        <div className="rev-category-row">
                <button>Upcoming</button>
                <button>Completed</button>
        </div>
        <div className="rev-cards-section">
                                    <div className="review-card">
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
                                                    <div className="rev-date-label">Date</div>
                                                    <div className="rev-date"> - </div>
                                                </div>
                                            </div>
            
                                            <div className="rev-date-container">
                                                <div className="rev-icon-container">
                                                    <icons.IoTimeOutline/>
                                                </div>
                                                <div className="rev-content-wrapper">
                                                    <div className="rev-date-label">Time</div>
                                                    <div className="rev-date"></div>
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
                                                    <div className="rev-date"></div>
                                                </div>
                                            </div>
            
                                            <div className="rev-date-container">
                                                <div className="rev-icon-container">
                                                    <icons.IoPersonOutline/>
                                                </div>
                                                <div className="rev-content-wrapper">
                                                    <div className="rev-date-label">Supervisor</div>
                                                    <div className="rev-date"></div>
                                                </div>
                                            </div>
                                        </div>
                                            <div className="rev-description">
                                                <h2> Review Description</h2>
                                                <p></p>
                                            </div>
                                                                                
                                        <div className="involved-students-row">
                                            <strong>Involved Students: </strong> 
                                        </div>
                                        <div className="students-row">
                                                <div className="student-name-container">
                                                    
                                                </div>
                                            
                                        </div>
                                        
                                        
                                    </div>
                                
                        
                        
        </div>

        </>

    )
    
}