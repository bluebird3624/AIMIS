import api from './api';

// Constants for sessionStorage keys
const ACCESS_TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';
const USER_DATA_KEY = 'user_data';
const NAME_IDENTIFIER_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
const EMAIL_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
const NAME_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";  


/**
 * 
 * HELPER FUNCTIONS E.G. DECODING THE JWT TOKEN AND ACCESSING USER DATA
 */

const decodeJwtPayload = (token) => {
    try {
      
        const parts = token.split('.');
        if (parts.length !== 3) {
            console.error("Invalid JWT format: Token must have 3 parts.");
            return null;
        }
       
        const encodedPayload = parts[1];
        
        let base64 = encodedPayload.replace(/-/g, '+').replace(/_/g, '/');
       
        while (base64.length % 4) {
            base64 += '=';
        }

        const jsonPayload = atob(base64);

        return JSON.parse(jsonPayload);
        
    } catch (e) {
        // This catches errors from malformed Base64 or invalid JSON
        console.error("Failed to decode or parse token payload:", e);
        return null;
    }
};

const constructUserData = (token) => {
    const rawPayload = decodeJwtPayload(token);

    if (!rawPayload) {
        return null;
    }

    const userData = {
      
        userId: rawPayload.sub || rawPayload[NAME_IDENTIFIER_CLAIM],
        email: rawPayload.email || rawPayload[EMAIL_CLAIM],
        username: rawPayload[NAME_CLAIM],
        role: 'admin' || rawPayload[ROLE_CLAIM] ,
        
        expiryTime: rawPayload.exp ? rawPayload.exp * 1000 : null,
    };

    return userData;
};
/**
 * 
 * 
 * USER AUTH FUNCTIONS
 * 
 * 
 * 
 * 
*/
export const login = async (credentials) => {
  try {
    // console.log(`login route called: ${credentials.email} + ${credentials.password}`);
    // Validate input
    if (!credentials.email || !credentials.password) {
      throw new Error('Email and password are required');
    }
    
    // Make API call to .NET backend
    const response = await api.post('/auth/login', {
      email: credentials.email,
      password: credentials.password
    });

    console.log('login respose', response);
    // Extract data from .NET response
    const { accessToken, refreshToken, expiresAtUtc } = response.data;
    const userData = constructUserData(accessToken);

    // Store tokens using sessionStorage
    setToken(accessToken);
    if (refreshToken) {
      setRefreshToken(refreshToken);
    }

    
    sessionStorage.setItem(USER_DATA_KEY, JSON.stringify(userData));

    // Return user data for Redux state
    return {
      user: userData,
      accessToken,
      expiresAtUtc
    };

  } catch (error) {
    // Transform and throw error for Redux to handle
    throw transformAuthError(error);
  }
};

export const register = async (credentials) => 
{
   try 
   {

    const {firstName, lastName, email, password, confirmPassword} = credentials;
    

   }
   catch (error)
   {
    throw transformAuthError(error);
   } 
}

export const logout = async () => {
  try {
    
    const currentRefreshToken = getRefreshToken();
    const accessToken = getToken();
   
    if (currentRefreshToken) {
      try {
       
        await api.post('/auth/logout', {"refreshToken": currentRefreshToken}, {
          headers: {
            'Authorization': `Bearer ${accessToken}`
          },
          
        });
        console.log('Server logout successful');
      } catch (apiError) {
       
        console.warn('Server logout failed, proceeding with local cleanup:', apiError.message);
      }
    }

    
    removeTokens();
    
    return {
      success: true,
      message: 'Logout successful'
    };

  } catch (error) {
   
    console.error('Logout error:', error);
    removeTokens();
   
    return {
      success: true,
      message: 'Logout completed (local cleanup)'
    };
  }
};


/**
 * 
 * JWT TOKEN HANDLER
 * 
 * 
 * 
 * 
 */
// Get user data from JWT token

export const decodeJWT = (token) => {
  try {
    if (!token || !isValidJWTFormat(token)) {
      return null;
    }

    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload;
  } catch (error) {
    console.error('Error decoding JWT:', error.message);
    return null;
  }
};

export const getUserFromJWT = (token = null) => {
  try {
    const jwtToken = token || getToken();
    if (!jwtToken) return null;

    const payload = decodeJWT(jwtToken);
    if (!payload) return null;

    // Extract user information from JWT payload
    // Adjust these field names based on your .NET JWT structure
    return {
      id: payload.sub || payload.userId || payload.id,
      email: payload.email || payload.Email,
      firstName: payload.given_name || payload.firstName || payload.FirstName,
      lastName: payload.family_name || payload.lastName || payload.LastName,
      role: payload.role || payload.Role,
      permissions: payload.permissions || payload.Permissions || [],
      departmentId: payload.departmentId || payload.DepartmentId,
      exp: payload.exp,
      iat: payload.iat
    };
  } catch (error) {
    console.error('Error extracting user from JWT:', error.message);
    return null;
  }
};


export const isAuthenticated = () => {
  const token = getToken();
  return token !== null && isJWTValid(token);
};

export const getCurrentUser = () => {
  try {
    // First try to get user data from sessionStorage (faster)
    const storedUserData = sessionStorage.getItem(USER_DATA_KEY);
    if (storedUserData) {
      const userData = JSON.parse(storedUserData);
      // Verify token is still valid
      if (isAuthenticated()) {
        return userData;
      }
    }

    // Fallback to extracting from JWT token
    return getUserFromJWT();
  } catch (error) {
    console.error('Error getting current user:', error.message);
    return null;
  }
};


export const setToken = (token) => {
  try {
    if (!token) {
      console.warn('Attempted to store null/undefined token');
      return false;
    }

    // Validate JWT token format
    if (!isValidJWTFormat(token)) {
      throw new Error('Invalid JWT token format');
    }

    // Store token in sessionStorage (cleared when browser/tab closes)
    sessionStorage.setItem(ACCESS_TOKEN_KEY, token);

    // Store token metadata for debugging/tracking
    const tokenData = {
      storedAt: Date.now(),
      expiresAt: getJWTExpiry(token),
      issuedAt: getJWTIssuedAt(token)
    };
    sessionStorage.setItem(`${ACCESS_TOKEN_KEY}_meta`, JSON.stringify(tokenData));

    return true;

  } catch (error) {
    console.error('Error storing JWT token:', error.message);
    // Clean up any partial storage
    sessionStorage.removeItem(ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(`${ACCESS_TOKEN_KEY}_meta`);
    return false;
  }
};


export const getToken = () => {
  try {
    const token = sessionStorage.getItem(ACCESS_TOKEN_KEY);
    
    if (!token) {
      return null;
    }

    // Validate JWT token and check expiry
    if (isJWTValid(token)) {
      return token;
    }
    
    // Clean up expired/invalid token
    removeTokens();
    return null;

  } catch (error) {
    console.error('Error retrieving JWT token:', error.message);
    removeTokens(); // Clean up on error
    return null;
  }
};


export const setRefreshToken = (refreshToken) => {
  try {
    if (refreshToken) {
      sessionStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
      return true;
    }
    return false;
  } catch (error) {
    console.error('Error storing refresh token:', error.message);
    return false;
  }
};


export const getRefreshToken = () => {
  try {
    return sessionStorage.getItem(REFRESH_TOKEN_KEY);
  } catch (error) {
    console.error('Error retrieving refresh token:', error.message);
    return null;
  }
};


export const removeTokens = () => {
  try {
    sessionStorage.removeItem(ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(REFRESH_TOKEN_KEY);
    sessionStorage.removeItem(USER_DATA_KEY);
    sessionStorage.removeItem(`${ACCESS_TOKEN_KEY}_meta`);
  } catch (error) {
    console.error('Error removing tokens:', error.message);
  }
};


const isValidJWTFormat = (token) => {
  try {
    const parts = token.split('.');
    return parts.length === 3 && parts.every(part => part.length > 0);
  } catch (error) {
    return false;
  }
};

const isJWTValid = (token) => {
  try {
    if (!isValidJWTFormat(token)) return false;
    
    // Check if JWT is expired
    const expiry = getJWTExpiry(token);
    if (expiry && Date.now() >= expiry) {
      return false;
    }
    
    return true;
  } catch (error) {
    return false;
  }
};


const getJWTExpiry = (token) => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp ? payload.exp * 1000 : null; // Convert to milliseconds
  } catch (error) {
    console.warn('Could not decode JWT expiry:', error.message);
    return null;
  }
};


const getJWTIssuedAt = (token) => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.iat ? payload.iat * 1000 : null; // Convert to milliseconds
  } catch (error) {
    console.warn('Could not decode JWT issued at:', error.message);
    return null;
  }
};




/**
 * 
 * ERROR HANDLING FUNCTIONS
 * 
 * 
 * 
 * 
 */

export const transformAuthError = (error) => {
  // Clean up tokens on auth errors
  if (error.response?.status === 401) {
    removeTokens();
  }

  // Handle network errors
  if (!error.response) {
    console.log('undefined error: ', error);
    return {
      type: 'NETWORK_ERROR',
      message: 'Unable to connect to server. Please check your internet connection.',
      statusCode: null,
      details: null
    };
  }

  const { status, data } = error.response;

  // Handle different HTTP status codes (same as before)
  switch (status) {
    case 400:
      if (data.errors) {
        const validationErrors = {};
        Object.keys(data.errors).forEach(field => {
          validationErrors[field.toLowerCase()] = data.errors[field];
        });

        return {
          type: 'VALIDATION_ERROR',
          message: 'Please check your input and try again.',
          statusCode: 400,
          details: validationErrors
        };
      }

      return {
        type: 'BAD_REQUEST',
        message: data.message || 'Invalid request. Please check your input.',
        statusCode: 400,
        details: null
      };

    case 401:
      return {
        type: 'AUTH_ERROR',
        message: data.message || 'Invalid email or password.',
        statusCode: 401,
        details: null
      };

    case 403:
      return {
        type: 'FORBIDDEN',
        message: data.message || 'You do not have permission to access this resource.',
        statusCode: 403,
        details: null
      };

    case 500:
    default:
      return {
        type: 'SERVER_ERROR',
        message: 'An unexpected error occurred. Please try again later.',
        statusCode: status,
        details: process.env.NODE_ENV === 'development' ? data : null
      };
  }
};