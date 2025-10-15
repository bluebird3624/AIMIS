import * as icons from 'react-icons/io5';
import React, { useState, useEffect } from 'react';
import '../../Styles/dashboards.css';
import { useNavigate, useLocation } from 'react-router-dom';

function Homebutton() {
  const [greeting, setGreeting] = useState('');
  const [userName, setUserName] = useState('');
  const [pendingAssignments, setPendingAssignments] = useState([]);
  const [upcomingAbsences, setUpcomingAbsences] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [currentTime, setCurrentTime] = useState(new Date());

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
      <div className="activity-container">
        <div className="active-tasks">
          <div className="icon-container">
            <icons.IoPeopleOutline style={{ marginTop: "7px" }} />
          </div>
          <div className="description-words">Total Students</div>
          <span className='description-number'>9</span>
        </div>
        <div className="active-tasks">
          <div className="icon-container">
            <icons.IoCheckmarkCircleOutline style={{ marginTop: "7px" }} />
          </div>
          <div className="description-words">Complete Tasks</div>
          <span className='description-number'>0</span>
        </div>
        <div className="active-tasks">
          <div className="icon-container">
            <icons.IoDocumentTextOutline style={{ marginTop: "7px" }} />
          </div>
          <div className="description-words">Reviews</div>
          <span className='description-number'>0</span>
        </div>
        <div className="active-tasks">
          <div className="icon-container">
            <icons.IoWalkOutline style={{ marginTop: "7px" }} />
          </div>
          <div className="description-words">Absences</div>
          <span className='description-number'>0</span>
        </div>
      </div>
      <div className="firstrow-container">
        <div className='smaller-container'>
          <h1 style={{ fontFamily: "arial", margin: "5px 0 0 5px" }}>Ongoing Assignments</h1>
          <p style={{ fontFamily: "arial", color: "#7f7f7f", margin: "5px 0 0 10px" }}>Requiring your attention:</p>
        </div>
        <div className='smaller-container'>
          <h1 style={{ fontFamily: "arial", margin: "5px 0 0 5px" }}>Upcoming Reviews</h1>
          <p style={{ fontFamily: "arial", color: "#7f7f7f", margin: "5px 0 0 10px" }}>Scheduled evaluation sessions:</p>
        </div>
      </div>
       <div className="secondrow-container">
        
      </div>
    </>
  );
}

export default Homebutton;