import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { IoSearch, IoClose } from 'react-icons/io5';

const SearchBar = ({ sidebarItems, userRole }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [isActive, setIsActive] = useState(false);
  const [searchResults, setSearchResults] = useState([]);
  const searchRef = useRef(null);
  const navigate = useNavigate();

  
  const searchData = [
   
    ...sidebarItems.map(item => ({
      type: 'navigation',
      title: item.name,
      subtitle: 'Navigate to',
      path: item.path,
      icon: item.iconOutline
    })),
    
    // Quick actions
    {
      type: 'action',
      title: 'Profile Settings',
      subtitle: 'View and edit your profile',
      action: () => navigate('/profile'),
      icon: 'person'
    },
    {
      type: 'action',
      title: 'Logout',
      subtitle: 'Sign out of your account',
      action: () => {
        sessionStorage.clear();
        navigate('/login');
      },
      icon: 'logout'
    },
    
    // Feature descriptions
    {
      type: 'feature',
      title: 'User Management',
      subtitle: 'Manage system users and permissions',
      path: '/users',
      roles: ['ADMIN', 'HR']
    },
    {
      type: 'feature',
      title: 'Absence Tracking',
      subtitle: 'Track and manage employee absences',
      path: '/absence',
      roles: ['ADMIN', 'HR']
    }
  ].filter(item => !item.roles || item.roles.includes(userRole));

  // Search logic
  useEffect(() => {
    if (searchTerm.trim()) {
      const results = searchData.filter(item =>
        item.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.subtitle.toLowerCase().includes(searchTerm.toLowerCase())
      ).slice(0, 8); // Limit to 8 results
      
      setSearchResults(results);
    } else {
      setSearchResults([]);
    }
  }, [searchTerm, sidebarItems]);

  // Handle search result click
  const handleResultClick = (result) => {
    if (result.action) {
      result.action();
    } else if (result.path) {
      navigate(result.path);
    }
    setSearchTerm('');
    setIsActive(false);
  };

  // Handle keyboard shortcuts
  useEffect(() => {
    const handleKeyPress = (e) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        searchRef.current?.focus();
        setIsActive(true);
      }
      if (e.key === 'Escape') {
        setIsActive(false);
        searchRef.current?.blur();
      }
    };

    document.addEventListener('keydown', handleKeyPress);
    return () => document.removeEventListener('keydown', handleKeyPress);
  }, []);

  return (
    <div className="search-container">
      <div className={`search-bar ${isActive ? 'active' : ''}`}>
        <IoSearch className="search-icon" />
        <input
          ref={searchRef}
          type="text"
          className="search-input"
          placeholder="Search anything... (Ctrl+K)"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onFocus={() => setIsActive(true)}
          onBlur={() => setTimeout(() => setIsActive(false), 200)}
        />
        {searchTerm && (
          <button
            className="search-clear"
            onClick={() => {
              setSearchTerm('');
              setSearchResults([]);
            }}
          >
            <IoClose />
          </button>
        )}
      </div>

      {/* Search Results */}
      {isActive && searchResults.length > 0 && (
        <div className="search-results">
          <div className="search-results-header">
            <span>Search Results</span>
            <span className="result-count">{searchResults.length} found</span>
          </div>
          {searchResults.map((result, index) => (
            <div
              key={index}
              className={`search-result-item ${result.type}`}
              onClick={() => handleResultClick(result)}
            >
              <div className="result-content">
                <div className="result-title">{result.title}</div>
                <div className="result-subtitle">{result.subtitle}</div>
              </div>
              <div className="result-type-badge">{result.type}</div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default SearchBar;