
import * as icons from 'react-icons/io5';
import React, { useState, useEffect } from 'react';
import '../Styles/dashboards.css'
import { useNavigate, useLocation } from 'react-router-dom';
import { getCurrentUser } from '../services/auth';

function Homebutton() {
  const [greeting, setGreeting] = useState('');
  const [userName, setUserName] = useState('');
  const [pendingAssignments, setPendingAssignments] = useState([]);
  const [upcomingAbsences, setUpcomingAbsences] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [currentTime, setCurrentTime] = useState(new Date());


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

  // Step 1: Greeting with time and name functionality
  useEffect(() => {
    // Set up real-time clock
    const timer = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);

    // Initialize user name and greeting
    initializeUserAndGreeting();

    return () => clearInterval(timer);
  }, []);

  const initializeUserAndGreeting = () => {
    // Get user name from localStorage or prompt
    const savedName = localStorage.getItem('userName');
    if (savedName) {
      setUserName(savedName);
    } else {
      const name = prompt('What should we call you?') || 'Friend';
      setUserName(name);
      localStorage.setItem('userName', name);
    }

    // Set initial greeting based on current time
    updateGreeting();

    setIsLoading(false);
  };

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

  const handleNameChange = () => {
    const newName = prompt('What should we call you?', userName) || 'Friend';
    setUserName(newName);
    localStorage.setItem('userName', newName);
  };

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
    <>
      <div className="greeting-message">
        <h1>
          {greeting}
          <span onClick={handleNameChange}>
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
            <div className="label">Students</div>
            <div className="number">9</div>
          </div>
        </div>
        <div className="stats-container">
          <div className="icon-container">
            <icons.IoClipboardOutline style={{ fontSize: "40px" }} />
          </div>
          <div className="content-wrapper">
            <div className="label">Active tasks</div>
            <div className="number">0</div>
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
      <div className='adminlarge-container'>
        <div className='adminsmall-container'>
          <p className='adminsmall-container-label'> Notifications</p>

        </div>
        <div className='adminsmall-container'>
          <h1 className='adminsmall-container-label'> Performance summaries</h1>

        </div>


      </div>
    </>
  );
}

export default Homebutton;