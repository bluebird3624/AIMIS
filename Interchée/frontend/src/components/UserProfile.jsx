import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {logout} from '../services/auth';
import { IoPersonOutline, IoLogOutOutline, IoChevronDown, IoSettingsOutline, IoLogoFacebook } from 'react-icons/io5';

const UserProfile = ({setActiveItem}) => {
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [userData, setUserData] = useState(null);
  const dropdownRef = useRef(null);
  const navigate = useNavigate();
  const profileComponentId = 'profilePage';

  // Get user data
  useEffect(() => {
    const getUserData = () => {
      try {
        const storedData = sessionStorage.getItem('user_data');
        if (storedData) {
          const user = JSON.parse(storedData);
          setUserData(user);
        } else {
          setUserData({
            firstName: 'User',
            lastName: '',
            email: 'user@example.com',
            role: 'Employee'
          });
        }
      } catch (error) {
        console.error('Error getting user data:', error);
        setUserData({
          firstName: 'User',
          lastName: '',
          email: 'user@example.com',
          role: 'Employee'
        });
      }
    };

    getUserData();
  }, []);

  
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleProfileClick = () => {
    setDropdownOpen(false);
    setActiveItem(profileComponentId);
    
  };


  const handleLogout = async () => {
    try {
      setDropdownOpen(false);
      await logout();
      
      sessionStorage.clear();
      navigate('/login');
    } catch (error) {
      console.error('Logout failed:', error);
      sessionStorage.clear();
      navigate('/login');
    }
  };

  const toggleDropdown = () => {
    setDropdownOpen(!dropdownOpen);
  };

  if (!userData) return null;

  const userInitials = `${userData.username?.[0] || 'U'}`;
  const fullName = `${userData.username}`.trim();

  return (
    <div className="user-profile-section" ref={dropdownRef}>
      <div className="user-avatar" onClick={toggleDropdown}>
        {userInitials}
      </div>
      
      <div className="user-info" onClick={toggleDropdown}>
        <div className="user-name">{fullName}</div>
        <div className="user-role">{userData.role}</div>
      </div>
      
      <div className="profile-dropdown">
        <IoChevronDown
          className={`dropdown-arrow ${dropdownOpen ? 'open' : ''}`}
          onClick={toggleDropdown}
        />
        
        {dropdownOpen && (
          <div className="dropdown-menu">
            <div className="dropdown-header">
              <div className="dropdown-user-info">
                <div className="dropdown-user-name">{fullName}</div>
                <div className="dropdown-user-email">{userData.email}</div>
              </div>
            </div>
            
            <div className="dropdown-divider"></div>
            
            <div className="dropdown-item" onClick={handleProfileClick}>
              <IoPersonOutline className="dropdown-icon" />
              <span>View Profile</span>
            </div>
                        
            <div className="dropdown-divider"></div>
            
            <div className="dropdown-item logout" onClick={handleLogout}>
              <IoLogOutOutline className="dropdown-icon" />
              <span>Logout</span>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default UserProfile;