import { useNavigate } from "react-router-dom";
import  '../Styles/welcome.css';

function WelcomePage() {
  const welcomestyle = {
    
  };

  const navigate = useNavigate()

  const handleClick = () => {
    navigate('/Onboarding') 
  }

  return (
  <div className="body-welcome " style={{ backgroundImage : "url('/src/assets/RegistratioPage.png')" } }>
    <div className='welcome-container'>
    <p className='welcome-message'>
      WELCOME TO AWESOME <p style={{ color : 'gold'}}>AGILE</p>
    </p>
  
    </div>
 
    <div className='button-container'>

    <button className='enroll-button' onClick={handleClick}>
        Enroll
    </button>
    <button className='aboutus-button'>
      About Us
    </button>
    </div>
  

  </div>


  );
  

 
}

export default WelcomePage;
