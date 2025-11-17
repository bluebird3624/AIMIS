import axios from 'axios';

const env = import.meta.env.VITE_ENV;

4

const BACKEND_API_URL = import.meta.env.VITE_APP_BACKEND_API_URL;
const api = axios.create({
    baseURL: "http://172.20.2.140:5007/",
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
    getUserByDepartment: ({params}) => 
        api.get('/users', {params}),
    
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

export const rubricAPI = {
    getAllRubrics: () =>
        api.get(`/rubrics`),
    createRubric: (formData) =>
        api.post('/rubrics', formData)
}

export const assignmentAPI = {
    getAllAssignments: () =>
        api.get('/assignments'),
    createAssignment: (assignmentData) =>
        api.post('/assignments', assignmentData),
    getMyAssignments: () =>
        api.get('/assignments/my-assignments'),
    assignAssignment: (assignmentId, selectedInterns) =>
        api.post(`/assignments/${assignmentId}/assign`, selectedInterns),
    getSpecificAssignment: (id) =>
        api.get(`/assignments/${id}`),
    updateAssignment: (id,assignmentData) =>
        api.put(`/assignments/${id}`, assignmentData),
    deleteAssignment: (id) =>
        api.delete(`/assignments/${id}`),
    updateAssignmentStatus: (id, status) =>
        api.put(`/assignments/${id}/status`, status),
    getAssignmentProgress: (id) =>
        api.get(`/assignments/${id}/progress`)
}

export const attachmentAPI = {

}

export const submissionAPI = {
    makeGithubSubmission: (assignmentId, repoUrl) =>
        api.post(`/submissions`, assignmentId, repoUrl),
    makeFileSubmission: (assignmentId, fileUrl) => 
        api.post('/submissions/file', assignmentId, fileUrl),
    fetchSpecificSubmission: (id) =>
        api.get(`/submissions/assignment/${id}`),
    fetchMySubmissions: () =>
        api.get('/submissions/my-submissions'),
    addCommit: (submissionId, commitData) =>
        api.post(`/submissions/${submissionId}/commits`, commitData),
    getCommits: (submissionId) =>
        api.get(`/submissions/${submissionId}/commits`),
    getAssignmentSubmissionsAll: (assignmentId) =>
        api.get(`/submissions/assignment/${assignmentId}/all`),
    updateGitSubmission: (submissionId, updateData) =>
        api.put(`/submissions/${submissionId}`, updateData),
    deleteSubmission: (submissionId) =>
        api.delete(`/submissions/${submissionId}`),
    updateSubmissionStatus: (submissionId, status) =>
        api.put(`/submissions/${submissionId}/status`, status)




    }

    export const ReviewAPI = {
        createReview: (reviewData) => 
            api.post('/reviews/bulk', reviewData),
        fetchReviews: (id) =>
            api.get(`/reviews/user/${id}`)
        
    }



export default api;
