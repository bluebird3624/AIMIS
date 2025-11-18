import React, { useState } from 'react';
import '../Styles/profilePage.css';
import * as icons from 'react-icons/io5';
import defaultProfile from '../assets/default.png'; 

const ProfilePage = () => {
  const raw = sessionStorage.getItem('user_data');
  const user = raw ? JSON.parse(raw) : {
    username: 'John Doe',
    role: 'Attachee',
    email: 'john.doe@example.com',
    dob: '1990-01-01',
    phone: '+123456789'
  };

  const [isChangePasswordModalOpen, setIsChangePasswordModalOpen] = useState(false);
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [passwordErrors, setPasswordErrors] = useState({});

  // Fallback function in case the image fails to load
  const handleImageError = (e) => {
    e.target.src = defaultProfile;
  };

  const handlePasswordChange = (field, value) => {
    setPasswordData(prev => ({
      ...prev,
      [field]: value
    }));
    
    // Clear error when user starts typing
    if (passwordErrors[field]) {
      setPasswordErrors(prev => ({
        ...prev,
        [field]: ''
      }));
    }
  };

  const validatePasswordForm = () => {
    const errors = {};

    if (!passwordData.currentPassword.trim()) {
      errors.currentPassword = 'Current password is required';
    }

    if (!passwordData.newPassword.trim()) {
      errors.newPassword = 'New password is required';
    } else if (passwordData.newPassword.length < 6) {
      errors.newPassword = 'Password must be at least 6 characters';
    }

    if (!passwordData.confirmPassword.trim()) {
      errors.confirmPassword = 'Please confirm your new password';
    } else if (passwordData.newPassword !== passwordData.confirmPassword) {
      errors.confirmPassword = 'Passwords do not match';
    }

    setPasswordErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmitPasswordChange = async (e) => {
    e.preventDefault();
    
    if (!validatePasswordForm()) {
      return;
    }

    try {
      // Here you would call your API to change the password
      console.log('Changing password...', {
        currentPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword
      });

      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 1000));

      // Reset form and close modal on success
      setPasswordData({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      });
      setIsChangePasswordModalOpen(false);
      
      alert('Password changed successfully!');
      
    } catch (error) {
      console.error('Error changing password:', error);
      setPasswordErrors({ 
        submit: 'Failed to change password. Please try again.' 
      });
    }
  };

  const handleCloseModal = () => {
    setIsChangePasswordModalOpen(false);
    setPasswordData({
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    });
    setPasswordErrors({});
  };

  return (
    <>
      <div className="mainProfileContainer">
        <div className="profileHeader">
          <h2>Profile</h2>
          <button 
            className="change-password-btn"
            onClick={() => setIsChangePasswordModalOpen(true)}
          >
            <icons.IoKeyOutline /> Change Password
          </button>
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

      {/* Change Password Modal */}
      {isChangePasswordModalOpen && (
        <div className="rev-modal-overlay">
          <div className="rev-modal-container">
            <div className="rev-modal-header">
              <h2 className="rev-modal-title">Change Password</h2>
              <button 
                className="rev-modal-close"
                onClick={handleCloseModal}
              >
                <icons.IoClose />
              </button>
            </div>
            
            <form onSubmit={handleSubmitPasswordChange}>
              <div className="rev-modal-content">
                <div className="rev-form-group">
                  <label className="rev-form-label">Current Password *</label>
                  <input
                    type="password"
                    className="rev-form-input"
                    placeholder="Enter your current password"
                    value={passwordData.currentPassword}
                    onChange={(e) => handlePasswordChange('currentPassword', e.target.value)}
                  />
                  {passwordErrors.currentPassword && (
                    <div className="password-error">{passwordErrors.currentPassword}</div>
                  )}
                </div>

                <div className="rev-form-group">
                  <label className="rev-form-label">New Password *</label>
                  <input
                    type="password"
                    className="rev-form-input"
                    placeholder="Enter new password"
                    value={passwordData.newPassword}
                    onChange={(e) => handlePasswordChange('newPassword', e.target.value)}
                  />
                  {passwordErrors.newPassword && (
                    <div className="password-error">{passwordErrors.newPassword}</div>
                  )}
                </div>

                <div className="rev-form-group">
                  <label className="rev-form-label">Confirm New Password *</label>
                  <input
                    type="password"
                    className="rev-form-input"
                    placeholder="Confirm new password"
                    value={passwordData.confirmPassword}
                    onChange={(e) => handlePasswordChange('confirmPassword', e.target.value)}
                  />
                  {passwordErrors.confirmPassword && (
                    <div className="password-error">{passwordErrors.confirmPassword}</div>
                  )}
                </div>

                {passwordErrors.submit && (
                  <div className="password-error submit-error">{passwordErrors.submit}</div>
                )}
              </div>

              <div className="rev-modal-actions">
                <button 
                  type="button"
                  className="rev-cancel-btn"
                  onClick={handleCloseModal}
                >
                  Cancel
                </button>
                <button 
                  type="submit"
                  className="rev-submit-btn"
                >
                  Change Password
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </>
  );
};

export default ProfilePage;