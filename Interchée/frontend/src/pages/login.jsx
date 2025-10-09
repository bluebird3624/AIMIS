import { useState } from "react";
import '../Styles/login.css';
import { useNavigate } from "react-router-dom";
import { login } from "../services/auth";
import { motion } from 'framer-motion';
import  LoginGroup from '../assets/LoginGroup.svg';



function Login(){
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    

    const handleClick = async(event) => {
      event.preventDefault();
      try
        {
            console.log(event);
          const email = document.getElementById('email').value;
          const password = document.getElementById('password').value;
         
          const response = await login({email, password});
        // console.log('login page login response: ', response);
        if(response.accessToken){
            navigate('/admin-dash');
        }
        else
        {
            console.log('error logging in no access token received', response);
        }
      
          
       
      }
      catch(error)
      {
        console.error('login failed', error);
      }
      
   }
  

    return(
        <motion.div
            initial={{ x: '100%' }}
           animate={{ x: 0 }}
            exit={{ x: '-100%' }}
            transition={{ type: 'keyframes', duration: 2.1 }}
            className="absolute inset-0"

        >
            <div className="body-login"    style={{ backgroundImage: "url('/src/assets/loginpage.png')"}}>
                <div className="login-gradient">
                    <div className="agile-logo">
                        <img
                        src={LoginGroup}
                        />
                    </div>
                    <div className="aimis-name">
                        Attaché Intern Management
                        & Information System
                    </div>
                </div>
                <div className="login-form">
                    <div className="form-container">
    <h2 className="form-title">Welcome to Agile AIMIS</h2>
    
    <div className="input-group">
      <label className="input-label">Email Address </label>
      <input 
        type="email" 
        className="form-input" 
        placeholder="Enter your email"
        id = "email"
      />
    </div>
    
    <div className="input-group">
      <label className="input-label">Password</label>
      <input 
        type="password" 
        className="form-input" 
        placeholder="Enter your password"
        id="password"
      />
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
      
      <a href="#" className="forgot-password">
        Forgot password?
      </a>
    </div>
    
    <button 
    onClick={handleClick}
    className="login-button">
      Log In
    </button>
    
   
  </div>
                </div>
                
            </div>
        </motion.div>
    );
}

export default Login;