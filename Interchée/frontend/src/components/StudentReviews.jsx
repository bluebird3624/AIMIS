import { useState, useEffect } from "react";
import * as icons from 'react-icons/io5';
import '../styles/reviews.css'; 
import { ReviewAPI, usersAPI } from "../services/api";
import { getCurrentUser } from "../services/auth";

export default function StudentReviews() {
    
    const [reviews, setReviews] = useState([]);
    const [users, setUsers] = useState([]);

    const fetchReviews = async () => {
        const user = getCurrentUser();
        const response = await ReviewAPI.fetchReviews(user.userId);
        console.log(response);
        setReviews(response.data);

    }

    const fetchUsers = async () => {
        const response = await usersAPI.getUsers();
        setUsers(response.data);
             
        
    }

    const fetchSupervisorName =  (supervisorId) => {
       const user = users.find(u => u.id === supervisorId);
        return user? user.userName : "user not found";
    }

    useEffect(()=> {
        fetchReviews();
        fetchUsers();
    },[]);




    return (
        <>
        <h1 style={{ fontFamily:"arial", fontSize: "35px", marginLeft: "20px"}}> Reviews </h1>
        <div className="rev-category-row">
                <button>Upcoming</button>
                <button>Completed</button>
        </div>
        <div className="rev-cards-section">
                                {reviews.map((review, index) => {

                                    const supervisorName = fetchSupervisorName(review.supervisorId);
                                    return (
                                    <div className="review-card"
                                        key={index}
                                    >
                                        <div className="review-card-toprow">
                                            <p className="card-revtitle">{review.title}</p>
                                            <span className="rev-status-badge"></span>
                                        </div>
                                        
                                        <div className="card-statsrow">
                                            <div className="rev-date-container">
                                                <div className="rev-icon-container">
                                                    <icons.IoCalendarNumberOutline/>
                                                </div>
                                                <div className="rev-content-wrapper">
                                                    <div className="rev-date-label">Sheduled at</div>
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
                                                    <div className="rev-date">{supervisorName}</div>
                                                </div>
                                            </div>
                                        </div>
                                            <div className="rev-description">
                                                <h2> Review Description</h2>
                                                <p>{review.description}</p>
                                            </div>
                                                                                
                                      
                                        
                                        
                                    </div> 
                                    )})}
                                
                        
                        
        </div>

        </>

    )
    
}