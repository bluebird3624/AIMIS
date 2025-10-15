import '../Styles/dashboards.css'
import { motion } from 'framer-motion';
import  * as icons from 'react-icons/io5';
import React, {useState, useEffect} from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {roles} from '../utils/constants';
import SearchBar from '../components/SearchBar';
import UserProfile from '../components/UserProfile';
import Group from '../assets/Group.svg';


const sidebarConfig = {
  adminDashboard: {
    id: 'adminDashboard',
    name: 'Home',
    path: '/admin-dash',
    iconOutline: icons.IoHomeOutline,
    iconSolid: icons.IoHome,
    roles: [roles.ADMIN, roles.HR]
  },


  attacheeDashboard: {
    id: 'attacheeDashboard',
    name: 'Home',
    path: '/attachee-dash',
    iconOutline: icons.IoHomeOutline,
    iconSolid: icons.IoHome,
    roles: [roles.ATTACHEE]
  },


  internDashboard: {
    id: 'internDashboard',
    name: 'Home',
    path: '/intern-dash',
    iconOutline: icons.IoHomeOutline,
    iconSolid: icons.IoHome,
    roles: [roles.INTERN]
  },


  supervisorDashboard: {
    id: 'supervisorDashboard',
    name: 'Home',
    path: '/supervisor-dash',
    iconOutline: icons.IoHomeOutline,
    iconSolid: icons.IoHome,
    roles: [roles.SUPERVISOR]
  },


  
  users: {
    id: 'users',
    name: 'Users',
    path: '/users',
    iconOutline: icons.IoPeopleOutline,
    iconSolid: icons.IoPeople,
    roles: [roles.ADMIN, roles.HR]
  },



  absence: {
    id: 'absence',
    name: 'Absence',
    path: '/absence',
    iconOutline: icons.IoWalkOutline,
    iconSolid: icons.IoWalk,
    roles: [roles.ADMIN, roles.HR]
  },



  reports: {
    id: 'reports',
    name: 'Reports',
    path: '/reports',
    iconOutline: icons.IoBarChartOutline,
    iconSolid: icons.IoBarChart,
    roles: [roles.ADMIN, roles.ATTACHEE, roles.HR, roles.INTERN, roles.SUPERVISOR]
  },


  notifications: {
    id: 'notifications',
    name: 'Notifications',
    path: '/notifications',
    iconOutline: icons.IoNotificationsOutline,
    iconSolid: icons.IoNotifications,
    roles: [roles.ADMIN, roles.ATTACHEE, roles.HR, roles.INTERN, roles.SUPERVISOR]
  },

  calendar: {
    id: 'calendar',
    name: 'Calendar',
    path: '/calendar',
    iconOutline: icons.IoCalendarOutline,
    iconSolid: icons.IoCalendar,
    roles: [roles.ADMIN, roles.ATTACHEE, roles.HR, roles.INTERN, roles.SUPERVISOR]

  }

};

const getUserRole = () => {
  try {
    const userData = sessionStorage.getItem('user_data');
    if (userData) {
      const user = JSON.parse(userData);
      return user.role || 'invalid'; 
    }
    return 'invalid';
  } catch (error) {
    console.error('Error getting user role:', error);
    return 'Employee';
  }
};



const getFilteredSidebarItems = (userRole) => {
  return Object.values(sidebarConfig).filter(item => 
    item.roles.includes(userRole)
  );
};

const SidebarItem = ({ item, isActive, onClick }) => {
  const [isHovered, setIsHovered] = useState(false);
  const IconOutline = item.iconOutline;
  const IconSolid = item.iconSolid;

  return (
    <button 
      className={`sidebar-options ${isActive ? 'active' : ''} ${isHovered ? 'hover' : ''}`}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      onClick={() => onClick(item)}
    > 
      {isHovered || isActive? <IconSolid /> : <IconOutline />}
      {item.name}
    </button>
   
  );
};


function AdminDash() {
  const [activeItem, setActiveItem] = useState('dashboard');
  const [sidebarItems, setSidebarItems] = useState([]);
  const [userRole, setUserRole] = useState('');
  const navigate = useNavigate();
  const location = useLocation();

 
  useEffect(() => {
    const userRole = getUserRole();
    setUserRole(userRole);
    const filteredItems = getFilteredSidebarItems(userRole);
    setSidebarItems(filteredItems);
  }, []);

 
  useEffect(() => {
    const currentItem = Object.values(sidebarConfig).find(
      item => item.path === location.pathname
    );
  
    if (currentItem) {
      setActiveItem(currentItem.id);
    }
  }, [location.pathname]);


  const handleItemClick = (item) => {
    setActiveItem(item.id);
    navigate(item.path);
  };

  return (
    <motion.div
      initial={{ x: '100%' }}
      animate={{ x: 0 }}
      exit={{ x: '-100%' }}
      transition={{ type : 'tween' , duration: 2.1  }}
      className="absolute inset-0"
    >
      
    <div className="dashboard-layout">
      {/* Header Section */}
      <div className="dashboard-header">
        {/* Logo Section */}
         <div className="header-logo">
          <img src={Group} alt="Company Logo" className="logo-image" />
        </div>

        {/* Search Section */}
        <div className="header-search">
          <SearchBar sidebarItems={sidebarItems} userRole={userRole} />
        </div>

        {/* User Profile Section */}
        <div className="header-profile">
          <UserProfile />
        </div>
      </div>

      {/* Main Content Area */}
      <div className="dashboard-main">
        {/* Sidebar Navigation */}
         
        <div className="dashboard-sidebar">
         
          <div className="options-container">
            {sidebarItems.map((item) => (
              <SidebarItem
                key={item.id}
                item={item}
                isActive={activeItem === item.id}
                onClick={handleItemClick}
              />
            ))}
          </div>
        </div>

        <div className='main-content'>
          
        </div>

       
      </div>
    </div>
   </motion.div>
  );
  
}

export default AdminDash;


 