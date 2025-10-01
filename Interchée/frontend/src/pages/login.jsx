import { useState } from "react";
import '../Styles/login.css';
import { useNavigate } from "react-router-dom";


function Login(){
    const navigate = useNavigate();

    const handleClick = () => {
        navigate('/admin-dash') ;
   }





    return(
        <>
        
        <div className="body-login" style={{ backgroundImage: "url('/src/assets/loginpage.png')"}}>
            <div className="login-container">
  <div className="login-header">
    <p style={{ color: 'white'}} >Sign in to your account</p>
  </div>
  
  <form className="login-form">
    {/* Email Field */}
    <div className="form-group">
      <label htmlFor="email" className="login-form-label">Email Address</label>
      <input
        type="email"
        id="email"
        className="form-input"
        placeholder="Enter your email"
        required
      />
    </div>

    {/* Password Field */}
    <div className="form-group">
      <label htmlFor="password" className="login-form-label">Password</label>
      <input
        type="password"
        id="password"
        className="form-input"
        placeholder="Enter your password"
        required
      />
    </div>

    {/* Login Button */}
    <button type="submit" className="login-button" onClick={handleClick}>
      Log In
    </button>

    {/* Forgot Password Link */}
    <div className="forgot-password">
      <a href="/forgot-password" className="forgot-link">
        Forgot your password?
      </a>
    </div>
  </form>
</div>
        </div>
        </>
    );
}
export default Login;