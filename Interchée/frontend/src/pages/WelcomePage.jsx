import { useNavigate } from "react-router-dom";
import  '../Styles/welcome.css';

function WelcomePage() {
  const welcomestyle = {
    
  };

  const navigate = useNavigate()

  const handleClick = () => {
    navigate('/login') ;
  }

  const redirect = () => {
    window.location.href = 'https://self-onboarding.agilebiz.co.ke/home';
  };

  return (
  <div className="body-welcome " style={{ backgroundImage : "url('/src/assets/RegistratioPage.png')" } }>
    <div className='welcome-container'>
    <p className='welcome-message'>
      WELCOME TO AWESOME <p style={{ color : 'gold'}}>AGILE</p>
    </p>
  
    </div>
 
    <div className='button-container'>

    <button className='enroll-button' onClick={handleClick}>
        Log in
    </button>
    <button className='aboutus-button' onClick={redirect}>
      About Us
    </button>
    </div>
  

  </div>


  );
  

 
}

export default WelcomePage;
