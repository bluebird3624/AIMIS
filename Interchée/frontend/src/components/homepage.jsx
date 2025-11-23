import * as icons from 'react-icons/io5';
import React, { useState, useEffect } from 'react';
import '../Styles/dashboards.css'
import { useNavigate, useLocation } from 'react-router-dom';
import { getCurrentUser } from '../services/auth';
import { motion } from 'framer-motion';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import { Bar } from 'react-chartjs-2';

// Register ChartJS components
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

function Homebutton() {
  const [greeting, setGreeting] = useState('');
  const [userName, setUserName] = useState('');
  const [pendingAssignments, setPendingAssignments] = useState([]);
  const [upcomingAbsences, setUpcomingAbsences] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [currentTime, setCurrentTime] = useState(new Date());

  // Bar chart data state
  const [chartData, setChartData] = useState(null);

  const getUserRole = () => {
    try {
      const userData = getCurrentUser();
      setUserName(userData.userName)
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
    getUserRole();
    initializeChartData();
  }, []);

  // Initialize chart data with sample department data
  const initializeChartData = () => {
    // Sample data - replace with actual API data
    const departments = [
      'Engineering',
      'Marketing', 
      'Sales',
      'HR',
      'Finance',
      'Operations',
      'IT'
    ];
    
    const studentCounts = [15, 8, 12, 6, 4, 10, 7];

    const data = {
      labels: departments,
      datasets: [
        {
          label: 'Number of Students',
          data: studentCounts,
          backgroundColor: [
            '#167aa1',
            '#167aa1',
            '#167aa1',
            '#167aa1',
            '#167aa1',
            '#167aa1',
            '#167aa1'
          ],
        
          borderRadius: 8,
          borderSkipped: false,
        },
      ],
    };

    setChartData(data);
  };

  // Chart options
  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top',
        labels: {
          font: {
            size: 12,
            family: "'Arial', sans-serif"
          },
          color: '#333'
        }
      },
      
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        titleColor: '#fff',
        bodyColor: '#fff',
        borderColor: '#fff',
        borderWidth: 1,
        cornerRadius: 8,
        displayColors: true,
        callbacks: {
          label: function(context) {
            return `Students: ${context.parsed.y}`;
          }
        }
      }
    },
    scales: {
      x: {
        grid: {
          display: false
        },
        ticks: {
          font: {
            size: 11,
            family: "'Arial', sans-serif"
          },
          color: '#666'
        }
      },
      y: {
        beginAtZero: true,
        grid: {
          color: 'rgba(0, 0, 0, 0.1)'
        },
        ticks: {
          font: {
            size: 11,
            family: "'Arial', sans-serif"
          },
          color: '#666',
          stepSize: 5
        },
        title: {
          display: true,
          text: 'Number of Students',
          font: {
            size: 18,
            family: "'Arial', sans-serif",
            weight: 'bold'
          },
          color: '#000000ff'
        }
      },
    },
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    }
  };

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
           <p className='adminsmall-container-label'>Notifications</p>

        </div>
        <div className='adminsmall-container'>
          <p className='adminsmall-container-label'>Departmental Distribution</p>
          <div style={{ 
            height: '400px', 
            padding: '20px',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center'
          }}>
            {chartData ? (
              <Bar 
                data={chartData} 
                options={chartOptions}
                style={{ 
                  maxHeight: '100%', 
                  maxWidth: '100%' 
                }}
              />
            ) : (
              <div style={{ 
                display: 'flex', 
                justifyContent: 'center', 
                alignItems: 'center', 
                height: '100%',
                color: '#666',
                fontFamily: 'Arial, sans-serif'
              }}>
                Loading chart...
              </div>
            )}
          </div>
        </div>
      </div>
    </motion.div>
  );
}

export default Homebutton;