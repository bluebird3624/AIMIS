import { useState } from "react";
import '../Styles/login.css';
import { useNavigate } from "react-router-dom";
import * as icons from 'react-icons/io5';
import * as auth from '../services/auth';
import { authAPI, departmentAPI, onboardAPI } from "../services/api";

function OnboardingForm() {
  const [formData, setFormData] = useState({
    firstName: '',
    middleName: '',
    lastName: '',
    role: '',
    email: '',
    password: '',
    confirmPassword: ''
  });

 
  const navigate = useNavigate();
  const [errors, setErrors] = useState({});
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const departments = [
    { value: '', label: 'Select Department' },
    { value: 'bespoke', label: 'Bespoke Solutions' },
    { value: 'corporate', label: 'Corporate Services' },
    { value: 'microsoft', label: 'Microsoft Business' },
    { value: 'hr', label: 'Human Resources' },
    { value: 'infra', label: 'Infrastructure' },
    { value: 'oracle', label: 'Oracle Business' },
    { value: 'sap', label: 'SAP Business'}
  ];

  const roles = [
    { value: '', label: 'Select Role' },
    { value: 'intern', label: 'Intern' },
    { value: 'Attache', label: 'Attaché' },
    { value: 'Supervisor', label: ' Supervisor'}
  ];

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.firstName.trim()) newErrors.firstName = 'First name is required';
    if (!formData.lastName.trim()) newErrors.lastName = 'Last name is required';
    
    if (!formData.email.trim()) {
      newErrors.email = 'Email is required';
    } 
    else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      newErrors.email = 'Email is invalid';
    }
    
    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const newErrors = validateForm();
    
    if (Object.keys(newErrors).length === 0) {
      
      const response =  await onboardAPI.createOnboardingRequest({
        email: formData.email,
        firstName: formData.firstName, 
        lastName: formData.lastName,
        middleName: formData.middleName,
        departmentId: 1
        }
      );
      if(response.status == 200){
        navigate('/login');

      }          
    } else {
      setErrors(newErrors);
    }
  };

  return (
    <>
      <div className="body-onboarding" style={{ backgroundImage: "url('/src/assets/formpage.png')"}}>
        <div className="info-container">
          <form onSubmit={handleSubmit}>
            {/* Row 1: Names */}
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="firstName">First Name *</label>
                <input
                  type="text"
                  id="firstName"
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  className={errors.firstName ? 'error' : ''}
                  placeholder="Enter first name"
                />
                {errors.firstName && <span className="error-message">{errors.firstName}</span>}
              </div>
              
              <div className="form-group">
                <label htmlFor="middleName">Middle Name</label>
                <input
                  type="text"
                  id="middleName"
                  name="middleName"
                  value={formData.middleName}
                  onChange={handleChange}
                  placeholder="Enter middle name"
                />
              </div>
              
              <div className="form-group">
                <label htmlFor="lastName">Last Name *</label>
                <input
                  type="text"
                  id="lastName"
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  className={errors.lastName ? 'error' : ''}
                  placeholder="Enter last name"
                />
                {errors.lastName && <span className="error-message">{errors.lastName}</span>}
              </div>
            </div>

            {/* Row 2: Department, Role, Email */}
            <div className="form-row">
              
              
             
              
              <div className="form-group">
                <label htmlFor="email">Email *</label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  className={errors.email ? 'error' : ''}
                  placeholder="Enter email address"
                />
                {errors.email && <span className="error-message">{errors.email}</span>}
              </div>
            </div>


            <button type="submit" className="submit-btn">
              Submit
            </button>
          </form>
          <p style={{ fontSize: "20px", fontFamily: "arial"}}>Already have an account? <a href="/login"> Sign in</a></p>
        </div>
      </div>
    </>
  );
}

export default OnboardingForm;