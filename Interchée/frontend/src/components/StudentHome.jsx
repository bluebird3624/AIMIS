import * as icons from 'react-icons/io5';
import React, { useState, useEffect } from 'react';
import '../Styles/dashboards.css'
import { useNavigate, useLocation } from 'react-router-dom';
import { getCurrentUser } from '../services/auth';
import {motion} from 'framer-motion'

function StudentHome() {
  const [greeting, setGreeting] = useState('');
  const [userName, setUserName] = useState('');
  const [pendingAssignments, setPendingAssignments] = useState([]);
  const [upcomingAbsences, setUpcomingAbsences] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [currentTime, setCurrentTime] = useState(new Date());

  // Sample assignments data
  const sampleAssignments = [
    {
      id: 1,
      taskName: 'Research Paper on AI Ethics',
      dueDate: '2024-01-15',
      status: 'In Progress'
    },
    {
      id: 2,
      taskName: 'Mobile App Development',
      dueDate: '2024-01-20',
      status: 'Assigned'
    },
    {
      id: 4,
      taskName: 'Web API Integration',
      dueDate: '2024-01-25',
      status: 'In Progress'
    },
    {
      id: 5,
      taskName: 'Final Presentation',
      dueDate: '2024-02-01',
      status: 'Assigned'
    }
  ];

  const sampleReviews = [
    {
      id: 1,
      reviewName: 'Assignment module test',
      reviewDate: '2025-10-8',
      reviewTime: '10:00 am',
      revSupervisor: 'Waffl3'
    },
    {
      id: 2,
      reviewName: 'REST API functionality',
      reviewDate: '2025-10-8',
      reviewTime: '4:00 pm',
      revSupervisor: 'S0imo'
    },
    {
      id: 3,
      reviewName: '.NET backend ',
      reviewDate: '2025-12-17',
      reviewTime: '7:00 am',
      revSupervisor: 'R0y'
    },
  ];

  // Initialize user data
  const initializeUserData = () => {
    try {
      const userData = getCurrentUser();
      if (userData && userData.userName) {
        setUserName(userData.userName);
      }
    } catch (error) {
      console.error('Error getting user data:', error);
    }
  };

  const getUserRole = () => {
    try {
      const userData = getCurrentUser();
      if (userData) {
        return userData.role || 'invalid'; 
      }
      return 'invalid';
    } catch (error) {
      console.error('Error getting user role:', error);
      return 'Employee';
    }
  };

  useEffect(() => {
    initializeUserData();
    getUserRole();
    // Set sample assignments
    setPendingAssignments(sampleAssignments);
  }, []);

  // Step 1: Greeting with time and name functionality
  useEffect(() => {
    // Set up real-time clock
    const timer = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const updateGreeting = () => {
    const hour = currentTime.getHours();
    if (hour >= 5 && hour < 12) {
      setGreeting('Good Morning ');
    } else if (hour >= 12 && hour < 17) {
      setGreeting('Good Afternoon ');
    } else if (hour >= 17 && hour < 21) {
      setGreeting('Good Evening ');
    } else {
      setGreeting('Good Night ');
    }
  };

  // Update greeting when time changes (especially around transition hours)
  useEffect(() => {
    updateGreeting();
  }, [currentTime.getHours()]);

  const formatTime = (date) => {
    return date.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true
    });
  };

  const formatDate = (date) => {
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  // Function to get status text color
  const getStatusColor = (status) => {
    switch (status) {
      case 'In Progress':
        return '#3498db'; // Blue
      case 'Assigned':
        return '#e74c3c'; // Red
      default:
        return '#95a5a6'; // Gray
    }
  };

  return (
    <motion.div
      initial={{ x: -100, opacity: 0 }}
      animate={{ x: 0, opacity: 1 }}
      transition={{ 
        duration: 1.7,
        ease: [0.25, 0.46, 0.45, 0.94] 
      }}
    >
      <div className="greeting-message">
        <h1>
          {greeting}
          <span>
            {userName}
          </span>
        </h1>
      </div>
      <p style={{ fontFamily: 'arial', fontSize:'20px', marginLeft:'20px', color: '#3d3d3d'}}> Here's what's going on</p>
      <div className='stats-row'>
        <div className="stats-container">
          <div className="icon-container">
            <icons.IoBookOutline style={{ fontSize: "40px" }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Reviews</div>
            <div className="number">{sampleReviews.length}</div>
          </div>
        </div>
        <div className="stats-container">
          <div className="icon-container">
            <icons.IoClipboardOutline style={{ fontSize: "40px" }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Active tasks</div>
            <div className="number">{pendingAssignments.filter(task => task.status === 'In Progress').length}</div>
          </div>
        </div>
        <div className="stats-container">
          <div className="icon-container">
            <icons.IoWalkOutline style={{ fontSize: "40px" }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Absences</div>
            <div className="number">0</div>
          </div>
        </div>
      </div>


      
      <div className='studentlarge-container'>
        <div className='studentlarge-container-row'>
          <div className='studentsmall-container'>
            <p style={{ fontSize: "26px", fontFamily:"arial", fontWeight:"750", marginTop:"10px", marginLeft:"20px"}}> Ongoing assignments </p>
            
            {/* Assignments Table */}
            <div className="assignments-table-container">
              <table className="assignments-table">
                <thead>
                  <tr>
                    <th className="table-header">Task Name</th>
                    <th className="table-header">Due Date</th>
                    <th className="table-header">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {pendingAssignments.map((assignment) => (
                    <tr key={assignment.id} className="table-row">
                      <td className="table-cell task-name">{assignment.taskName}</td>
                      <td className="table-cell due-date">{assignment.dueDate}</td>
                      <td className="table-cell">
                        <span 
                          className="status-text"
                          style={{ color: getStatusColor(assignment.status) }}
                        >
                          {assignment.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
          </div> 
          <div className='studentsmall-container'>
            <p style={{ fontSize: "26px", fontFamily:"arial", fontWeight:"750", marginTop:"10px", marginLeft:"20px"}}> Notifications </p>
            <div className='assignments-table-container'>
              <div className='notifications-container'>
                <div className='notifications-icon'>
                  <icons.IoAlertCircleOutline/>
                </div>
                <p className='notification-title'> REST API assignment  </p>
                <p className='notification-time '> 3h </p>
              </div>

              <div className='notifications-container'>
                <div className='notifications-icon'>
                  <icons.IoAlertCircleOutline/>
                </div>
                <p className='notification-title'> New review  </p>
                <p className='notification-time '> 8h </p>
              </div>

              <div className='notifications-container'>
                <div className='notifications-icon'>
                  <icons.IoAlertCircleOutline/>
                </div>
                <p className='notification-title'> Absence request rejected </p>
                <p className='notification-time '> 2d </p>
              </div>

              <div className='notifications-container'>
                <div className='notifications-icon'>
                  <icons.IoAlertCircleOutline/>
                </div>
                <p className='notification-title'> Feedback reply </p>
                <p className='notification-time '> 30m </p>
              </div>
            </div>
          </div>
        </div>
        <div className='studentlarge-container-row'>
          <div className='studentsmall-container'>
            <p style={{ fontSize: "26px", fontFamily:"arial", fontWeight:"750", marginTop:"10px", marginLeft:"20px"}}> Upcoming reviews</p>
            
            {/* Reviews Table */}
            <div className="assignments-table-container">
              <table className="assignments-table">
                <thead>
                  <tr>
                    <th className="table-header">Title</th>
                    <th className="table-header">Date</th>
                    <th className="table-header">Time</th>
                    <th className="table-header">Supervisor</th>
                  </tr>
                </thead>
                <tbody>
                  {sampleReviews.map((review) => (
                    <tr key={review.id} className="table-row">
                      <td className="table-cell task-name">{review.reviewName}</td>
                      <td className="table-cell due-date">{review.reviewDate}</td>
                      <td className="table-cell due-date">{review.reviewTime}</td>
                      <td className="table-cell">
                        <span className="supervisor-name">
                          {review.revSupervisor}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          
          </div> 

          <div className='studentsmall-container'>
            <p style={{ fontSize: "26px", fontFamily:"arial", fontWeight:"750", marginTop:"10px", marginLeft:"20px"}}> Recent feedback </p>
            <div className='feedback-section'>
              <div className='feedback-container'>
                <p className='feedback-title'> Assignment regularity </p>
                <p className='feedback-reply'> Noted , the situation is being investigated </p>

              </div>

               <div className='feedback-container'>
                <p className='feedback-title'> Assignment regularity </p>
                <p className='feedback-reply'> Noted , the situation is being investigated </p>

              </div>

               <div className='feedback-container'>
                <p className='feedback-title'> Assignment regularity </p>
                <p className='feedback-reply'> Noted , the situation is being investigated </p>

              </div>

               <div className='feedback-container'>
                <p className='feedback-title'> Assignment regularity </p>
                <p className='feedback-reply'> Noted , the situation is being investigated </p>

              </div>

            </div>

          </div>
        </div>
      </div>
    </motion.div>
  );
}

export default StudentHome;