import React, { createContext, useContext, useEffect, useReducer } from 'react';
import * as authService from './auth'; 
const AUTH_KEYS = {
  TOKEN: 'access_token',
  USER: 'user_data'
};

const initialState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: true,
  error: null
};

function reducer(state, action) {
  switch (action.type) {
    case 'RESTORE':
      return { ...state, ...action.payload, isLoading: false, error: null };
    case 'LOGIN_SUCCESS':
      return { ...state, ...action.payload, isAuthenticated: true, isLoading: false, error: null };
    case 'LOGIN_FAILURE':
      return { ...state, isAuthenticated: false, token: null, user: null, isLoading: false, error: action.payload };
    case 'LOGOUT':
      return { ...initialState, isLoading: false };
    default:
      return state;
  }
}

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [state, dispatch] = useReducer(reducer, initialState);

  useEffect(() => {
    try {
      const token = (authService.getToken) ;
      const rawUser = (authService.getCurrentUser);
      dispatch({
        type: 'RESTORE',
        payload: {
          token,
          rawUser,
          isAuthenticated: !!token
        }
      });
    } catch (e) {
      dispatch({ type: 'RESTORE', payload: { token: null, user: null, isAuthenticated: false } });
    }
  }, []);

  const login = async (credentials) => {
    try {
     
      const result =  authService.login;
     
      const token = result.accessToken;
      const user = result.user;

      dispatch({ type: 'LOGIN_SUCCESS', payload: { token, user } });
      return { token, user };
    } catch (err) {
      dispatch({ type: 'LOGIN_FAILURE', payload: err?.message || err });
      throw err;
    }
  };

  const logout = async () => {
    try {
      if (authService.logout) await authService.logout();
    } catch (e) {
      
      console.warn('logout api error', e);
    } finally {
      
      sessionStorage.removeItem(AUTH_KEYS.TOKEN);
      sessionStorage.removeItem(AUTH_KEYS.USER);
      dispatch({ type: 'LOGOUT' });
    }
  };

  const hasRole = (required) => {
    if (!state.user || !state.user.role) return false;
    if (Array.isArray(required)) return required.includes(state.user.role);
    return state.user.role === required;
  };

  const hasAnyRole = (roles) => {
    if (!state.user || !state.user.role) return false;
    return roles.includes(state.user.role);
  };

  return (
    <AuthContext.Provider value={{
      ...state,
      login,
      logout,
      hasRole,
      hasAnyRole
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
};

export default AuthContext;