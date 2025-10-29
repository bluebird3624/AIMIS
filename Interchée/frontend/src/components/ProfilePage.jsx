import React from 'react';
import '../Styles/profilePage.css';

const ProfilePage = () => {
  const raw = sessionStorage.getItem('user_data');
  const user = raw ? JSON.parse(raw) : {
    username: 'John Doe',
    role: 'Attachee',
    email: 'john.doe@example.com',
    dob: '1990-01-01',
    employeeNumber: 'EMP-001',
    phone: '+123456789'
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
              src={user.avatar || '../assets/dashboards.png'}
              alt={user.username}
            />
          </div>

          <div className="profileNameRole">
            <h3>{user.username}</h3>
            <p className="role">{user.role}</p>
          </div>

          <div className="profileSummary">
            <h4>Summary</h4>
            <ul className="info-list">
              <li><span className="label">DOB</span><span className="value">{user.dob || ''}</span></li>
              <li><span className="label">Employee No.</span><span className="value">{user.employeeNumber || ''}</span></li>
              <li><span className="label">Email</span><span className="value">{user.email}</span></li>
              <li><span className="label">Phone</span><span className="value">{user.phone || ''}</span></li>
            </ul>
          </div>
        </aside>

        <section className="profileRightBody">
          <div className="profileSection">
            <h4>Personal Info</h4>
            <hr />
            <ul className="info-list">
              <li><span className="label">Full name</span><span className="value">{user.username}</span></li>
              <li><span className="label">Date of birth</span><span className="value">{user.dob || ''}</span></li>
              <li><span className="label">Email</span><span className="value">{user.email}</span></li>
              <li><span className="label">Phone</span><span className="value">{user.phone || ''}</span></li>
            </ul>
          </div>

          <div className="profileSection">
            <h4>Organization</h4>
            <hr />
            <ul className="info-list">
              <li><span className="label">Employee No.</span><span className="value">{user.employeeNumber || ''}</span></li>
              <li><span className="label">Department</span><span className="value">Product</span></li>
              <li><span className="label">Manager</span><span className="value">Jane Manager</span></li>
              <li><span className="label">Start date</span><span className="value">2022-01-10</span></li>
            </ul>
          </div>

          <div className="profileSection">
            <h4>Financial</h4>
            <hr />
            <ul className="info-list">
              <li><span className="label">Bank</span><span className="value">Acme Bank</span></li>
              <li><span className="label">Account No.</span><span className="value">****1234</span></li>
              <li><span className="label">Salary</span><span className="value">$3,200</span></li>
            </ul>
          </div>
        </section>
      </div>
    </div>
  );
};

export default ProfilePage;