import { useState } from "react";
import '../Styles/login.css';
import { useNavigate } from "react-router-dom";

function Onboarding() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    firstname: '',
    middlename: '',
    surname: '',
    role: '',
    idtype: '',
    idnumber: '',
    phonenumber: '',
    email: ''
  });
  const [errors, setErrors] = useState({});

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    
    const newErrors = {};
    if (!formData.firstname.trim()) newErrors.firstname = 'First name is required';
    if (!formData.surname.trim()) newErrors.surname = 'Surname is required';
    if (!formData.email.trim()) newErrors.email = 'Email is required';

    setErrors(newErrors);

    if (Object.keys(newErrors).length === 0) {
      console.log('Form submitted successfully!');
      window.location.href = 'https://self-onboarding.agilebiz.co.ke/home';
    }
  };

  return (
    <div className="body-login" style={{ backgroundImage: "url('/src/assets/formpage.png')"}}>
      <div className="info-container">b
        
        <form onSubmit={handleSubmit}>
          {/* Name Section - Horizontal Row */}
          <div className="form-row-horizontal">
            {/* First Name - Mandatory */}
            <div className="form-row">
              <label htmlFor="firstname" className="form-label"> * First Name : </label>
              <input
                type="text" 
                id="firstname"
                name="firstname"
                className={`form-input ${errors.firstname ? 'error' : ''}`}
                placeholder="First name"
                value={formData.firstname}
                onChange={handleChange}
              />
              {errors.firstname && <span className="error-message">{errors.firstname}</span>}
            </div>

            {/* Middle Name - Optional */}
            <div className="form-row">
              <label htmlFor="middlename" className="form-label">Middle Name : </label>
              <input
                type="text" 
                id="middlename"
                name="middlename"
                className="form-input"
                placeholder="Middle name"
                value={formData.middlename}
                onChange={handleChange}
              />
            </div>

            {/* Surname - Mandatory */}
            <div className="form-row">
              <label htmlFor="surname" className="form-label"> * Surname : </label>
              <input
                type="text" 
                id="surname"
                name="surname"
                className={`form-input ${errors.surname ? 'error' : ''}`}
                placeholder="Surname"
                value={formData.surname}
                onChange={handleChange}
              />
              {errors.surname && <span className="error-message">{errors.surname}</span>}
            </div>
          </div>
          

          {/* Contact Section - Horizontal Row */}
          <div className="form-row-horizontal">
            {/* Email - Mandatory */}
            <div className="form-row">
              <label htmlFor="email" className="form-label"> * Email : </label>
              <input
                type="email"
                id="email"
                name="email"
                className={`form-input ${errors.email ? 'error' : ''}`}
                placeholder="your.email@example.com"
                value={formData.email}
                onChange={handleChange}
              />
              {errors.email && <span className="error-message">{errors.email}</span>}
            </div>
      
          </div>

          <button type="submit" className="submit-button">Submit</button>
        </form>

        <p style={{ fontSize: '20px', fontFamily: 'serif'}}> * this enrollment is not conclusive for interns </p>
      </div>
    </div>
  );
}

export default Onboarding;