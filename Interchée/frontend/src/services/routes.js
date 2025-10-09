import api from './api';



export const authAPI = {
    login: (email, password) =>
        api.post('/auth/login', {email, password}),
    register: ({credentials}) => 
        api.post('/auth/register', {credentials}),
}