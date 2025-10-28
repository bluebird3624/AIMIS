import React, { useState, useEffect } from 'react';
import { departmentAPI, onboardAPI, usersAPI } from '../services/api';

function Userspage() {
  

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isAssignDeptOpen, setisAssignDeptOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [userToDelete, setUserToDelete] = useState(null);
  const [onboardingRequests, setOnboardingRequests] = useState(null);
  const [departments, setDepartments] = useState(null);
  const [users, setUsers] = useState(null);
  const [newUser, setNewUser] = useState({
    name: '',
    dept: '',
    role: '',
    email: ''
  });
  const [selectedDept, setSelectedDept] = useState('');
  const [selectedRole, setSelectedRole] = useState('');


  
 

  const roles = [
    { value: '', label: 'Select Role' },
    { value: 'Intern', label: 'Intern' },
    { value: 'Attaché', label: 'Attaché' },
    { value: 'Supervisor', label: 'Supervisor' }
  ];


  useEffect(() => {
    fetchUsers();
    fetchDepartments();
   
  },[]);

  const fetchDepartments = async () =>{
    const departments = await departmentAPI.getDepartments();
    setDepartments(departments.data);
  }

  const fetchUsers = async () => {
    const verifiedUsers = await usersAPI.getUsers();
    setUsers(verifiedUsers.data);
    const unverifiedUsers = await onboardAPI.getOnboardingRequests();
    setOnboardingRequests(unverifiedUsers.data);

  };
 
  const handleCreateUser = () => {
    if (newUser.name && newUser.dept && newUser.role && newUser.email) {
      const user = {
        id: Date.now(),
        ...newUser
      };
      setUsers([...users, user]);
      setNewUser({ name: '', dept: '', role: '', email: '' });
      setIsCreateModalOpen(false);
    } else {
      alert('Please fill all fields');
    }
  };

 
  const handleApproveUser = async () => {
    if (selectedDept) {
      const updatedUser = {
        ...currentUser,
        department: selectedDept
      };
    const roleName = selectedRole;
    const response  =await onboardAPI.approveOnboardingRequest(currentUser.id, {
      roleName,
      userName: currentUser.proposedUserName,
      tempPassword: 'User@123',
    });
    console.log('approve onboarding response: ', response);
    const userEmail = response.data.email;
    const users = await usersAPI.getUsers();
    const usersData = users.data;
    const specificUser = usersData.find(user => user.email === userEmail);
    const userId = specificUser.id;
    
    const assignResponse = await usersAPI.assignDepartment({
      userId,
      departmentId: parseInt(selectedDept, 10),
      roleName,
      
    }) ;
    console.log('assign dept response: ', assignResponse);
   
      setUsers(prevUsers => prevUsers.map(user => 
        user.id === currentUser.id ? updatedUser : user
      ));
      
      setisAssignDeptOpen(false);
      setSelectedDept('');
      setCurrentUser(null);
      
      alert('User approved successfully!');
    } else {
      alert('Please select a department');
    }
  };

  
  const handleDeleteClick = (user) => {
    setUserToDelete(user);
    setIsDeleteModalOpen(true);
  };

  const handleConfirmDelete = () => {
    setUsers(users.filter(user => user.id !== userToDelete.id));
    setIsDeleteModalOpen(false);
    setUserToDelete(null);
  };

  const handleCancelDelete = () => {
    setIsDeleteModalOpen(false);
    setUserToDelete(null);
  };

  
  const handleEditUser = (user) => {
    setCurrentUser(user);
    setIsEditModalOpen(true);
  };

 
  const handleAssignDept = (onboardingRequest) => {
    setCurrentUser(onboardingRequest);
    setisAssignDeptOpen(true);
  }

  const handleUpdateUser = () => {
    if (currentUser.name && currentUser.dept && currentUser.role && currentUser.email) {
      setUsers(users.map(user => 
        user.id === currentUser.id ? currentUser : user
      ));
      setIsEditModalOpen(false);
      setCurrentUser(null);
    } else {
      alert('Please fill all fields');
    }
  };

  return (
    <div className="user-management">
    
      <div className="header">
        <h1>User Management</h1>
        <button 
          className="create-btn"
          onClick={() => setIsCreateModalOpen(true)}
        >
          Create User
        </button>
      </div>

      
      <h2 style={{ fontFamily: "arial"}}>Verified Users</h2>
      <div className="table-container">
        <table className="users-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Department</th>
              <th>Role</th>
              <th>Email</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users?.map(user => (
              <tr key={user.id}>
                <td>{user.userName}</td>
                <td>{user.departmentName || ''}</td>
                <td>{user.role || ''}</td>
                <td>{user.email}</td>
                <td className="actions">
                  <button 
                    className="edit-button"
                    onClick={() => handleEditUser(user)}
                  >
                    Edit
                  </button>
                  <button 
                    className="delete-button"
                    onClick={() => handleDeleteClick(user)}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      
      <h2 style={{ fontFamily: "arial"}}>Unverified Users</h2>
      <div className="table-container">
        <table className="users-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Role</th>
              <th>Email</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {onboardingRequests?.map(onboardingRequest => (
              <tr key={onboardingRequest.id}>
                <td>{onboardingRequest.proposedUserName}</td>
                <td>{onboardingRequest.role || ''}</td>
                <td>{onboardingRequest.email}</td>
                <td className="actions">
                  <button 
                    className="assign-dept-button"
                    onClick={() => handleAssignDept(onboardingRequest)}
                  >
                   Assign Dept
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

    
      {isCreateModalOpen && (
        <div className="modal-overlay">
          <div className="modal">
            <h2>Create New User</h2>
            <div className="form-group">
              <label>Name:</label>
              <input
                type="text"
                value={newUser.name}
                onChange={(e) => setNewUser({...newUser, name: e.target.value})}
                placeholder="Enter name"
              />
            </div>
            <div className="form-group">
              <label>Department:</label>
              <select
                value={newUser.dept}
                onChange={(e) => setNewUser({...newUser, dept: e.target.value})}
              >
                {departments.map(dept => (
                  <option key={dept.value} value={dept.value}>
                    {dept.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Role:</label>
              <select
                value={newUser.role}
                onChange={(e) => setNewUser({...newUser, role: e.target.value})}
              >
                {roles.map(role => (
                  <option key={role.value} value={role.value}>
                    {role.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Email:</label>
              <input
                type="email"
                value={newUser.email}
                onChange={(e) => setNewUser({...newUser, email: e.target.value})}
                placeholder="Enter email"
              />
            </div>
            <div className="modal-actions">
              <button className="cancel-btn" onClick={() => setIsCreateModalOpen(false)}>
                Cancel
              </button>
              <button className="save-btn" onClick={handleCreateUser}>
                Create User
              </button>
            </div>
          </div>
        </div>
      )}

     
      {isAssignDeptOpen && (
        <div className="modal-overlay">
          <div className="modal">
            <h2>Assign {currentUser.firstName + ' ' + currentUser.lastName} a department</h2>
            <div className="form-group">
              <label>Department:</label>
              <select
                value={selectedDept}
                onChange={(e) => setSelectedDept(e.target.value)}
              >
                {departments.map(dept => (
                  <option key={dept.id} value={dept.id}>
                    {dept.name}
                  </option>
                ))}
              </select>

              <label>Role:</label>
              <select
                value={selectedRole}
                onChange={(e) => setSelectedRole(e.target.value)}
              >
                {roles.map(role => (
                  <option key={role.value} value={role.value}>
                    {role.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="modal-actions">
              <button className="cancel-btn" onClick={() => setisAssignDeptOpen(false)}>
                Cancel
              </button>
              <button className="save-btn" onClick={handleApproveUser}>
                Approve User
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit User Modal */}
      {isEditModalOpen && currentUser && (
        <div className="modal-overlay">
          <div className="modal">
            <h2>Edit User</h2>
            <div className="form-group">
              <label>Name:</label>
              <input
                type="text"
                value={currentUser.name}
                onChange={(e) => setCurrentUser({...currentUser, name: e.target.value})}
              />
            </div>
            <div className="form-group">
              <label>Department:</label>
              <select
                value={currentUser.dept}
                onChange={(e) => setCurrentUser({...currentUser, dept: e.target.value})}
              >
                {departments.map(dept => (
                  <option key={dept.value} value={dept.value}>
                    {dept.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Role:</label>
              <select
                value={currentUser.role}
                onChange={(e) => setCurrentUser({...currentUser, role: e.target.value})}
              >
                {roles.map(role => (
                  <option key={role.value} value={role.value}>
                    {role.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Email:</label>
              <input
                type="email"
                value={currentUser.email}
                onChange={(e) => setCurrentUser({...currentUser, email: e.target.value})}
              />
            </div>
            <div className="modal-actions">
              <button className="cancel-btn" onClick={() => setIsEditModalOpen(false)}>
                Cancel
              </button>
              <button className="save-btn" onClick={handleUpdateUser}>
                Update User
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {isDeleteModalOpen && userToDelete && (
        <div className="modal-overlay">
          <div className="modal">
            <h2>Confirm Delete</h2>
            <p>Are you sure you want to delete user <strong>{userToDelete.name}</strong>?</p>
            <div className="modal-actions">
              <button className="cancel-btn" onClick={handleCancelDelete}>
                Cancel
              </button>
              <button className="delete-btn" onClick={handleConfirmDelete}>
                Delete User
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default Userspage;