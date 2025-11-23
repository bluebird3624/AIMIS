import * as icons from 'react-icons/io5';
import React, { useState, useEffect } from 'react';
import '../Styles/dashboards.css'
import { useNavigate, useLocation } from 'react-router-dom';
import { getCurrentUser } from '../services/auth';
import {motion} from 'framer-motion'

function SupervisorHome() {
  const [greeting, setGreeting] = useState('');
  const [userName, setUserName] = useState('');
  const [pendingAssignments, setPendingAssignments] = useState([]);
  const [upcomingAbsences, setUpcomingAbsences] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [currentTime, setCurrentTime] = useState(new Date());

 
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
            <icons.IoPeopleOutline style={{ fontSize: "40px" }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Department</div>
            <div className="number">0</div>
          </div>
        </div>
        <div className="stats-container">
          <div className="icon-container">
            <icons.IoClipboardOutline style={{ fontSize: "40px" }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Active tasks</div>
            <div className="number">2</div>
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
               <p style={{ fontSize: "26px", fontFamily:"arial", fontWeight:"750", marginTop:"10px", marginLeft:"20px"}}> Recent Activities </p>

            </div>


            <div className='studentsmall-container'>
               <p style={{ fontSize: "26px", fontFamily:"arial", fontWeight:"750", marginTop:"10px", marginLeft:"20px"}}> Ongoing assignments </p>

            </div>
         </div>
        
        
      </div>
    </motion.div>
  );
}

export default SupervisorHome;