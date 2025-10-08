import { useState } from "react";
import '../Styles/login.css';
import { useNavigate } from "react-router-dom";
import { login } from "../services/auth";
import { motion } from 'framer-motion';



function Login(){
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    

    const handleClick = async(event) => {
      event.preventDefault();
      try
        {
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
            <div className="body-login"    style={{ backgroundImage: "url('/src/assets/loginpage.png')"}}
>
                <div className="login-container">
                    <div className="login-header">
                        <p style={{ color: 'white'}} >Sign in to your account</p>
                    </div>
                    
                    <form className="login-form" onSubmit={handleClick}>
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
                        <button 
                            type="submit" 
                            className="login-button" 
                            disabled={loading}
                        >
                            {loading ? 'Logging in...' : 'Log In'}
                        </button>


                        <div className="forgot-password">
                            <a href="/forgot-password" className="forgot-link">
                                Forgot your password?
                            </a>
                        </div>
                    </form>
                </div>
            </div>
        </motion.div>
    );
}

export default Login;