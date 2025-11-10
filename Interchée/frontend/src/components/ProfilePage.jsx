import React from 'react';
import '../Styles/profilePage.css';
import * as icons from 'react-icons/io5';
import defaultProfile from '../assets/default.png'; // Import the default image

const ProfilePage = () => {
  const raw = sessionStorage.getItem('user_data');
  const user = raw ? JSON.parse(raw) : {
    username: 'John Doe',
    role: 'Attachee',
    email: 'john.doe@example.com',
    dob: '1990-01-01',
    phone: '+123456789'
  };

  // Fallback function in case the image fails to load
  const handleImageError = (e) => {
    e.target.src = defaultProfile;
  };

  return (
    <div className="mainProfileContainer">
      <div className="profileHeader">
        <h2>Profile</h2>
      </div>

      <div className="profileBody">
        <aside className="profileLeftBody">
          <div className="profilePhoto">
            <img
              src={user.avatar || defaultProfile}
              alt={user.username}
              onError={handleImageError}
            />
          </div>
          <div>
            <button className='change-photo'>
              <icons.IoCameraReverseOutline/>
            </button>
          </div>

          <div className="profileNameRole">
            <h3>{user.username}</h3>
            <p className="role">{user.role}</p>
          </div>

          <div className="profileSummary">
            <h4>Summary</h4>
            <ul className="info-list">
              <li><span className="profile-label">DOB</span><span className="value">{user.dob || ''}</span></li>
              <li><span className="profile-label">Email</span><span className="value">{user.email}</span></li>
              <li><span className="profile-label">Phone</span><span className="value">{user.phone || ''}</span></li>
            </ul>
          </div>
        </aside>

        <section className="profileRightBody">
          <div className="profileSection">
            <h4>Personal Info</h4>
            <hr />
            <ul className="info-list">
              <li><span className="profile-label">Full name</span><span className="value">{user.username}</span></li>
              <li><span className="profile-label">Date of birth</span><span className="value">{user.dob || ''}</span></li>
              <li><span className="profile-label">Email</span><span className="value">{user.email}</span></li>
              <li><span className="profile-label">Phone</span><span className="value">{user.phone || ''}</span></li>
            </ul>
          </div>

          <div className="profileSection">
            <h4>Organization</h4>
            <hr />
            <ul className="info-list">
              <li><span className="profile-label">Department</span><span className="value">Product</span></li>
              <li><span className="profile-label">Manager</span><span className="value">Jane Manager</span></li>
              <li><span className="profile-label">Start date</span><span className="value">2022-01-10</span></li>
            </ul>
          </div>
        </section>
      </div>
    </div>
  );
};

export default ProfilePage;