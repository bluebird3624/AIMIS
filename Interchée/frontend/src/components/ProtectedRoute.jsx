import React from 'react';
import { useNavigate, Outlet } from 'react-router-dom';
import { useAuth } from '../services/authContext';

const ProtectedRoute = ({ allowedRoles = null, children = null }) => {
  const navigate = useNavigate();
  const { isAuthenticated, isLoading, hasAnyRole } = useAuth();
 
  if (isLoading) return null; 

  if (!isAuthenticated) {
    return navigate('/login');
  }

  if (allowedRoles && !hasAnyRole(allowedRoles)) {
    return navigate('/unauthorized');
  }

  return children ? children : <Outlet />;
};

export default ProtectedRoute;