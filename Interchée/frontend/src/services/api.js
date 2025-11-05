import axios from 'axios';

const env = import.meta.env.VITE_ENV;

4

const BACKEND_API_URL = import.meta.env.VITE_APP_BACKEND_API_URL;
const api = axios.create({
    baseURL: "http://localhost:5007/",
    timeout: 15000,
    headers: {
        'Accept':'application/json',
        'Content-Type':'application/json',
        'ngrok-skip-browser-warning': true,
    }
});

api.interceptors.request.use(
    (config) => {
        const token = sessionStorage.getItem('access_token');
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
    if(env == 'development'){
        console.log('backend response: ', response);
    }
    
    return response;
  },
  (error) => {
    
     return Promise.reject(error);
  }
);


export const authAPI = {
    login: (email, password) => 
        api.post('/auth/login', {email, password}),
}

export const onboardAPI = {

     createOnboardingRequest: (data) => 
        api.post('/onboarding-requests', data),
     approveOnboardingRequest: (id, data) => 
        api.post(`/onboarding-requests/${id}/approve`, data),
     getOnboardingRequests: () =>
        api.get('/onboarding-requests'),

}

export const departmentAPI = {
    getDepartments: () =>
        api.get('/departments'),
    
}

export const usersAPI = {
    getUsers: () => 
        api.get('/users'),
    createUser: (data) =>
        api.post('/users', data),
    assignDepartment: (data) =>
        api.post('/department-roles/assign', data),
    
}

export const absenceAPI = {
    getAbsenceRequestsAll: () =>
        api.get('/absence-requests'),
    getMyAbsenceRequests: () =>
        api.get(`/absence-requests/my`),
    creatAbsenceRequest: (requestInfo) => 
        api.post('/absence-requests', requestInfo),
    approveRejectAbsenceRequest: (id, decisionInfo) =>
        api.post(`/absence-requests/${id}/decision`, decisionInfo),
    getRequestsByDepartment: (id) => 
        api.get(`/absence-requests/department/${id}`)
}
export default api;
