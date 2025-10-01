import axios from 'axios';


const BACKEND_API_URL = 'https://lorrie-unoperative-corrine.ngrok-free.dev/';

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