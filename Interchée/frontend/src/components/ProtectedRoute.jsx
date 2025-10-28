import React, { useEffect } from 'react';
import { useNavigate, Outlet, Navigate } from 'react-router-dom';
import { useAuth } from '../services/authContext';

const ProtectedRoute = ({ allowedRoles = null, children = null }) => {
  const navigate = useNavigate();
  const { isAuthenticated, isLoading, hasAnyRole } = useAuth();
 
  useEffect(() => {
    if (!isAuthenticated) {
    navigate('/login');
   
  }

  if (allowedRoles && !hasAnyRole(allowedRoles)) {
    console.log('rerouting to unauthorized, ', allowedRoles + ' ' + isAuthenticated);
    navigate('/unauthorized');
    
  }
    
  }, [navigate]);

  if (isLoading) return null; 

  

  return children ? children : <Outlet />;
};

export default ProtectedRoute;