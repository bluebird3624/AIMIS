import axios from 'axios';


// Use environment variable for backend API URL. Set REACT_APP_BACKEND_API_URL in your .env file.
const BACKEND_API_URL = process.env.REACT_APP_BACKEND_API_URL || 'http://localhost:8000/';

const api = axios.create({
    baseURL: BACKEND_API_URL,
    timeout: 15000,
    headers: {
        'Accept':'application/json',
        'Content-Type':'application/json',
        'ngrok-skip-browser-warning': true,
    }
});

api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('authToken');
        if(token){
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        console.log('no token', error);
        return Promise.reject(error);
    } 
);

api.interceptors.response.use(
  (response) => {
    console.log(response);
    return response;
  },
  (error) => {
    
     return Promise.reject(error);
  }
);



export default api;

export const authAPI = {
    login: (email, password) =>
        api.post('/auth/login', {email, password})
}