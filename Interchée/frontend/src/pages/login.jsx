import { useState } from "react";
import '../Styles/login.css';
import { useNavigate, Navigate } from "react-router-dom";
import { motion } from 'framer-motion';
import LoginGroup from '../assets/LoginGroup.svg';
import * as authService from "../services/authContext";

function Login() {
    const [loading, setLoading] = useState(false);
    const [showForgotPassword, setShowForgotPassword] = useState(false);
    const [errors, setErrors] = useState({});
    const navigate = useNavigate();
    const {login} = authService.useAuth();

    
   
    const handleClick = async(event) => {
      event.preventDefault();
    

        // Clear errors if validation passes
        setErrors({});

        try {
            const email = document.getElementById('email').value
            const password = document.getElementById('password').value

            
            setLoading(true);
            const response = await login({ email, password });
            
            if (response.accessToken || response.token) {
                navigate('/admin-dash');
            } else {
                console.log('error logging in no access token received', response);
                setErrors({ general: "Login failed. Please check your credentials." });
            }
        } catch (error) {
            console.error('login failed', error);
            setErrors({ general: "Login failed. Please try again." });
        } finally {
            setLoading(false);
        }
    }

    const handleForgotPasswordClick = (event) => {
        event.preventDefault();
        setShowForgotPassword(true);
        setErrors({}); // Clear errors when switching forms
    }

    const handleBackToLogin = () => {
        setShowForgotPassword(false);
        setErrors({}); // Clear errors when switching forms
    }

    const handleForgotPasswordSubmit = (event) => {
        event.preventDefault();
        
        const email = document.getElementById('forgot-email').value;
        
        // Validate forgot password form
        const formErrors = validateForgotPasswordForm(email);
        
        if (Object.keys(formErrors).length > 0) {
            setErrors(formErrors);
            return;
        }

        // Clear errors if validation passes
        setErrors({});

        console.log('Password reset requested for:', email);
        // You would typically call an API here to send reset instructions
        alert(`Password reset instructions sent to ${email}`);
        setShowForgotPassword(false);
    }

    // Clear individual field error when user starts typing
    const clearError = (fieldName) => {
        setErrors(prev => ({
            ...prev,
            [fieldName]: ''
        }));
    };

    return (
        <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '-100%' }}
            transition={{ type: 'keyframes', duration: 2.1 }}
            className="absolute inset-0"
        >
            <div className="body-login" style={{ backgroundImage: "url('/src/assets/loginpage.png')" }}>
                <div className="login-gradient">
                    <div className="agile-logo">
                        <img src={LoginGroup} />
                    </div>
                    <div className="aimis-name">
                        Attaché Intern Management
                        & Information System
                    </div>
                </div>
                
                <div className="login-form">
                    {!showForgotPassword ? (
                        // LOGIN FORM
                        <div className="form-container">
                            <h2 className="form-title">Welcome to Agile AIMIS</h2>
                            
                            {/* General error message */}
                            {errors.general && (
                                <div className="error-message general-error">
                                    {errors.general}
                                </div>
                            )}
                            
                            <div className="input-group">
                                <label className="input-label">Email Address</label>
                                <input 
                                    type="email" 
                                    className={`form-input ${errors.email ? 'error' : ''}`}
                                    placeholder="Enter your email"
                                    id="email"
                                    onChange={() => clearError('email')}
                                />
                                {errors.email && (
                                    <div className="error-message">{errors.email}</div>
                                )}
                            </div>
                            
                            <div className="input-group">
                                <label className="input-label">Password</label>
                                <input 
                                    type="password" 
                                    className={`form-input ${errors.password ? 'error' : ''}`}
                                    placeholder="Enter your password"
                                    id="password"
                                    onChange={() => clearError('password')}
                                />
                                {errors.password && (
                                    <div className="error-message">{errors.password}</div>
                                )}
                            </div>
                            
                            <div className="form-options">
                                <div className="remember-me">
                                    <input 
                                        type="checkbox" 
                                        id="remember" 
                                        className="remember-checkbox"
                                    />
                                    <label htmlFor="remember" className="remember-label">
                                        Remember me
                                    </label>
                                </div>
                                
                                <a href="#" className="forgot-password" onClick={handleForgotPasswordClick}>
                                    Forgot password?
                                </a>
                            </div>
                            
                            <button 
                                onClick={handleClick} 
                                className="login-button"
                                disabled={loading}
                            >
                                {loading ? 'Logging in...' : 'Log In'}
                            </button>
                            <p style={{ fontSize: "20px", fontFamily: "arial" }}>
                                Don't have an account? <a href="/Onboarding"> Sign up</a>
                            </p>
                        </div>
                    ) : (
                        // FORGOT PASSWORD FORM
                        <div className="form-container">
                            <h2 className="form-title">Reset Your Password</h2>
                            <p style={{fontFamily: 'arial'}}>
                                Enter your email address and we'll send you instructions to reset your password.
                            </p>
                            
                            {/* Forgot password error message */}
                            {errors.general && (
                                <div className="error-message general-error">
                                    {errors.general}
                                </div>
                            )}
                            
                            <div className="input-group">
                                <label className="input-label">Email Address</label>
                                <input 
                                    type="email" 
                                    className={`form-input ${errors.forgotEmail ? 'error' : ''}`}
                                    placeholder="Enter your email"
                                    id="forgot-email"
                                    onChange={() => clearError('forgotEmail')}
                                />
                                {errors.forgotEmail && (
                                    <div className="error-message">{errors.forgotEmail}</div>
                                )}
                            </div>
                            
                            <button onClick={handleForgotPasswordSubmit} className="login-button">
                                Send Reset Link
                            </button>
                            
                            <button onClick={handleBackToLogin} className="back-to-login-button">
                                Back to Login
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </motion.div>
    );
    }

export default Login;