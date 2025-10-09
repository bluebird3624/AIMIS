import  * as icons from 'react-icons/io5';
import React, {useState, useEffect} from 'react';
import '../../Styles/dashboards.css';
import { useNavigate, useLocation } from 'react-router-dom';
import {roles} from '../../utils/constants';


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
      className={`sidebar-options ${isActive ? 'active' : ''} ${isHovered ? 'hovered' : ''}`}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      onClick={() => onClick(item)}
    > 
      {isHovered ? <IconSolid /> : <IconOutline />}
      {item.name}
    </button>
  );
};

function Sidebar() {
  const [activeItem, setActiveItem] = useState('dashboard');
  const [sidebarItems, setSidebarItems] = useState([]);
  const navigate = useNavigate();
  const location = useLocation();

 
  useEffect(() => {
    const userRole = getUserRole();
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

  // Handle sidebar item click
  const handleItemClick = (item) => {
    setActiveItem(item.id);
    navigate(item.path);
  };

  return (
    <div className="sidebar">
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
    );
    
}

export default Sidebar;
