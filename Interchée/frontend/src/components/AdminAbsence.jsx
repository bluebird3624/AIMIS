import React, { useState } from 'react';
import * as icons from 'react-icons/io5';


function Adminabsence() {

    return(
        <>
        <div className="greeting-message">
          <h1>
            Absence Management
          </h1>
        </div>
        <div className='stats-row'>
                <div className="absence-stats-container">
                  <div className="pending-icon-container">
                    <icons.IoHourglassOutline style={{ fontSize: "40px" }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="label">Pending</div>
                    <div className="number">9</div>
                  </div>
                </div>
                <div className="absence-stats-container">
                  <div className="approved-icon-container">
                    <icons.IoCheckmarkCircleOutline style={{ fontSize: "40px" }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="label">Approved</div>
                    <div className="number">0</div>
                  </div>
                </div>
                <div className="absence-stats-container">
                  <div className="rejected-icon-container">
                    <icons.IoCloseCircleOutline style={{ fontSize: "40px" }} />
                  </div>
                  <div className="content-wrapper">
                    <div className="label">Rejected</div>
                    <div className="number">0</div>
                  </div>
                </div>
              
              </div>
              <h2 style={{ fontFamily:" arial" , marginLeft : "20px"}}> Absence Requests</h2>
              <div className='absence-request-section'>

                
                <div className='request-container'>
                  <div className='top-row'>
                    <div className="request-user-info">
                        <div className="request-user-name">John Doe</div>
                        <div className="absence-reason">Doctor's appointment</div>
                    </div>
                    <div className='absence-status-pending'>
                        pending
                    </div>
                  </div>
                    <div className='dates-row'>
                        <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoCalendarNumberOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">Start Date</div>
                               <div className="date">2025-8-7</div>
                            </div>

                        </div>
                         <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoCalendarNumberOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">End Date</div>
                               <div className="date">2025-8-9</div>
                            </div>

                        </div>
                         <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoTimerOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">Duration</div>
                               <div className="date"> 2 Days</div>
                            </div>

                        </div>

                    </div>
                    <div className='button-row'>
                        <button className='request-button' style={{ backgroundColor : "#1aff00 "}}>
                            <icons.IoCheckmarkCircleOutline/>
                            Approve
                        </button>
                         <button className='request-button' style={{ backgroundColor : " #ff0000"}}>
                            <icons.IoCloseCircleOutline/>
                            Reject
                        </button>

                    </div>

                </div>



              </div>
              <h2 style={{ fontFamily:" arial" , marginLeft : "20px"}}> Recent Decisions</h2>
              <div className='recent-decision-section'>
                <div className='recent-decision-container'>
                    <div className='top-row'>
                    <div className="request-user-info">
                        <div className="request-user-name">John Doe</div>
                        <div className="absence-reason">Doctor's appointment</div>
                    </div>
                    <div className='absence-status-approved'>
                        approved
                    </div>
                  </div>
                    <div className='dates-row'>
                        <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoCalendarNumberOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">Start Date</div>
                               <div className="date">2025-8-7</div>
                            </div>

                        </div>
                         <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoCalendarNumberOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">End Date</div>
                               <div className="date">2025-8-9</div>
                            </div>

                        </div>
                         <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoTimerOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">Duration</div>
                               <div className="date"> 2 Days</div>
                            </div>

                        </div>

                    </div>

                </div>
                <div className='recent-decision-container'>
                    <div className='top-row'>
                    <div className="request-user-info">
                        <div className="request-user-name">John Doe</div>
                        <div className="absence-reason">Doctor's appointment</div>
                    </div>
                    <div className='absence-status-rejected'>
                        rejected
                    </div>
                  </div>
                    <div className='dates-row'>
                        <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoCalendarNumberOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">Start Date</div>
                               <div className="date">2025-8-7</div>
                            </div>

                        </div>
                         <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoCalendarNumberOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">End Date</div>
                               <div className="date">2025-8-9</div>
                            </div>

                        </div>
                         <div className='date-container'>
                            <div className='icon-container'>
                                <icons.IoTimerOutline style={{ fontSize: "40px" }}/>
                            </div>
                            <div className="content-wrapper">
                               <div className="date-label">Duration</div>
                               <div className="date"> 2 Days</div>
                            </div>

                        </div>

                    </div>

                </div>
                

              </div>
              


        </>
    );
}

export default Adminabsence;