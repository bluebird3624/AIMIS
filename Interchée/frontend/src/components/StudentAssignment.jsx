import React, { useState } from 'react';
import '../Styles/studentAssignment.css';
import * as icons from 'react-icons/io5';

const StudentAssignment = () => {
    const [filter, setFilter] = useState('all');
    
    const assignments = [
        {
            id: 1,
            title: 'React Component Development',
            status: 'Submitted',
            description: 'Create a responsive React component with proper state management and CSS styling. Include error handling and accessibility features.',
            dueDate: '2024-01-15',
            submissionType: 'git',
            supervisor: 'Sarah Johnson',
            assignedDate: '2024-01-01'
        },
        {
            id: 2,
            title: 'API Integration Project',
            status: 'Pending',
            description: 'Integrate with a third-party API and handle different response states including loading, success, and error states.',
            dueDate: '2024-01-20',
            submissionType: 'file',
            supervisor: 'Michael Chen',
            assignedDate: '2024-01-05'
        },
        {
            id: 3,
            title: 'Database Design Assignment',
            status: 'Graded',
            description: 'Design a normalized database schema for an e-commerce application with proper relationships and constraints.',
            dueDate: '2024-01-10',
            submissionType: 'git',
            supervisor: 'Emily Rodriguez',
            assignedDate: '2023-12-28',
            rubric: {
                name: 'Database Design Rubric',
                totalMarks: 85,
                criteria: [
                    {
                        name: 'Normalization',
                        description: 'Proper normalization up to 3NF',
                        marksOutOf: 30,
                        marksAwarded: 28
                    },
                    {
                        name: 'Relationships',
                        description: 'Appropriate primary and foreign keys',
                        marksOutOf: 25,
                        marksAwarded: 22
                    },
                    {
                        name: 'Constraints',
                        description: 'Proper use of constraints and data types',
                        marksOutOf: 20,
                        marksAwarded: 18
                    },
                    {
                        name: 'Documentation',
                        description: 'Clear documentation and ER diagrams',
                        marksOutOf: 15,
                        marksAwarded: 12
                    },
                    {
                        name: 'Best Practices',
                        description: 'Following database design best practices',
                        marksOutOf: 10,
                        marksAwarded: 5
                    }
                ],
                supervisorComment: "Excellent work on normalization and relationships! Your database design shows strong understanding of 3NF. However, I noticed some missing constraints that would ensure data integrity. The documentation could be more detailed with clearer ER diagrams. Overall, a solid submission with room for improvement in best practices implementation."
            }
        },
        {
            id: 4,
            title: 'Research Paper',
            status: 'Submitted',
            description: 'Write a comprehensive research paper on modern web development frameworks.',
            dueDate: '2024-01-25',
            submissionType: 'file',
            supervisor: 'David Kim',
            assignedDate: '2024-01-03'
        },
        {
            id: 5,
            title: 'Web Application Project',
            status: 'Graded',
            description: 'Build a full-stack web application with frontend and backend components.',
            dueDate: '2024-01-18',
            submissionType: 'git',
            supervisor: 'Alex Thompson',
            assignedDate: '2024-01-02',
            rubric: {
                name: 'Full-Stack Development Rubric',
                totalMarks: 92,
                criteria: [
                    {
                        name: 'Frontend Implementation',
                        description: 'React components and user interface',
                        marksOutOf: 30,
                        marksAwarded: 28
                    },
                    {
                        name: 'Backend API',
                        description: 'RESTful API design and implementation',
                        marksOutOf: 30,
                        marksAwarded: 30
                    },
                    {
                        name: 'Database Integration',
                        description: 'Database connectivity and operations',
                        marksOutOf: 20,
                        marksAwarded: 18
                    },
                    {
                        name: 'Code Quality',
                        description: 'Clean code and proper documentation',
                        marksOutOf: 10,
                        marksAwarded: 8
                    },
                    {
                        name: 'Deployment',
                        description: 'Application deployment and accessibility',
                        marksOutOf: 10,
                        marksAwarded: 8
                    }
                ],
                supervisorComment: "Outstanding work on the backend API - perfect score! The frontend implementation is very clean and user-friendly. The database integration works well, though there are some optimization opportunities. Code quality is good with clear documentation. The deployment process is smooth and the application is easily accessible. Great job overall!"
            }
        }
    ];

    // Filter assignments based on selected category
    const filteredAssignments = assignments.filter(assignment => {
        if (filter === 'all') return true;
        if (filter === 'pending') return assignment.status === 'Pending';
        if (filter === 'submitted') return assignment.status === 'Submitted';
        if (filter === 'graded') return assignment.status === 'Graded';
        return true;
    });

    const handleMakeSubmission = (assignmentId, submissionData) => {
        console.log('Making submission for assignment:', assignmentId, submissionData);
        // Add your submission logic here
        alert(`Submitting assignment ${assignmentId} with data: ${JSON.stringify(submissionData)}`);
    };

    const handleMakeResubmission = (assignmentId, submissionData) => {
        console.log('Making resubmission for assignment:', assignmentId, submissionData);
        // Add your resubmission logic here
        alert(`Resubmitting assignment ${assignmentId} with data: ${JSON.stringify(submissionData)}`);
    };

    const AssignmentCard = ({ assignment }) => {
        const [isExpanded, setIsExpanded] = useState(false);
        const [gitUrl, setGitUrl] = useState('');
        const [uploadedFiles, setUploadedFiles] = useState([]);
        const [isDragOver, setIsDragOver] = useState(false);

        const toggleExpand = () => {
            setIsExpanded(!isExpanded);
        };

        // Handle file drop
        const handleDrop = (e) => {
            e.preventDefault();
            setIsDragOver(false);
            
            const files = Array.from(e.dataTransfer.files);
            setUploadedFiles(prev => [...prev, ...files]);
        };

        // Handle file input change
        const handleFileInput = (e) => {
            const files = Array.from(e.target.files);
            setUploadedFiles(prev => [...prev, ...files]);
        };

        // Handle drag over
        const handleDragOver = (e) => {
            e.preventDefault();
            setIsDragOver(true);
        };

        // Handle drag leave
        const handleDragLeave = (e) => {
            e.preventDefault();
            setIsDragOver(false);
        };

        // Remove uploaded file
        const removeFile = (index) => {
            setUploadedFiles(prev => prev.filter((_, i) => i !== index));
        };

        // Handle submission
        const handleSubmit = () => {
            const submissionData = {
                assignmentId: assignment.id,
                gitUrl: assignment.submissionType === 'git' ? gitUrl : null,
                files: assignment.submissionType === 'file' ? uploadedFiles : null
            };

            if (assignment.status === 'Pending') {
                handleMakeSubmission(assignment.id, submissionData);
            } else if (assignment.status === 'Submitted') {
                handleMakeResubmission(assignment.id, submissionData);
            }
        };

        // Determine which icon and text to show based on submission type
        const getSubmissionInfo = () => {
            switch (assignment.submissionType) {
                case 'git':
                    return {
                        icon: <icons.IoGitBranchOutline />,
                        text: 'Git Repository',
                        className: 'git-status'
                    };
                case 'file':
                    return {
                        icon: <icons.IoDocumentOutline/>,
                        text: 'File Upload',
                        className: 'file-status'
                    };
                default:
                    return {
                        icon: <icons.IoDocument />,
                        text: 'Submission',
                        className: 'default-status'
                    };
            }
        };

        const submissionInfo = getSubmissionInfo();

        return (
            <div className={`assignment-card ${isExpanded ? 'expanded' : ''}`}>
                <div className="assignment-header">
                    <div className="assignment-title-section">
                        <h3 className="assignment-title">{assignment.title}</h3>
                        <div className="assignment-meta">
                            <div className="supervisor-info">
                                <icons.IoPersonOutline className="meta-icon" />
                                <span>{assignment.supervisor}</span>
                            </div>
                            <div className="assigned-date-info">
                                <icons.IoCalendarOutline className="meta-icon" />
                                <span>{assignment.assignedDate}</span>
                            </div>
                        </div>
                    </div>
                    <div className="status-containers">
                        <div className={`status ${assignment.status.toLowerCase()}`}>
                            {assignment.status}
                        </div>
                        <div className={`submission-type ${submissionInfo.className}`}>
                            {submissionInfo.icon}
                            <span>{submissionInfo.text}</span>
                        </div>
                    </div>
                    <button 
                        className="view-details-btn"
                        onClick={toggleExpand}
                    >
                        {isExpanded ? 'View Less' : 'View Details'}
                        {isExpanded ? <icons.IoChevronUp /> : <icons.IoChevronDown />}
                    </button>
                </div>
                {isExpanded && (
                    <div className="assignment-expanded-content">
                        <div className="assignment-description">
                            <p>{assignment.description}</p>
                        </div>
                        
                        {/* Submission Form - Only for Pending and Submitted assignments */}
                        {(assignment.status === 'Pending' || assignment.status === 'Submitted') && (
                            <div className="submission-form">
                                <h4 className="submission-title">
                                    {assignment.status === 'Pending' ? 'Make Submission' : 'Make Resubmission'}
                                </h4>
                                
                                {/* Git Repository Input */}
                                {assignment.submissionType === 'git' && (
                                    <div className="git-submission-section">
                                        <label className="input-label">
                                            <icons.IoGitBranchOutline className="label-icon" />
                                            Git Repository URL
                                        </label>
                                        <input
                                            type="url"
                                            className="git-url-input"
                                            placeholder=" enter the repo url e.g. https://github.com/username/repository.git"
                                            value={gitUrl}
                                            onChange={(e) => setGitUrl(e.target.value)}
                                        />
                                        <p className="input-help">
                                            Enter the URL of your Git repository
                                        </p>
                                    </div>
                                )}
                                
                                {/* File Upload Section */}
                                {assignment.submissionType === 'file' && (
                                    <div className="file-submission-section">
                                        <label className="input-label">
                                            <icons.IoDocumentOutline className="label-icon" />
                                            Upload Files
                                        </label>
                                        
                                        {/* File Drop Zone */}
                                        <div 
                                            className={`file-drop-zone ${isDragOver ? 'drag-over' : ''} ${uploadedFiles.length > 0 ? 'has-files' : ''}`}
                                            onDrop={handleDrop}
                                            onDragOver={handleDragOver}
                                            onDragLeave={handleDragLeave}
                                        >
                                            <div className="drop-zone-content">
                                                <icons.IoCloudUploadOutline className="upload-icon" />
                                                <p className="drop-zone-text">
                                                    Drag and drop your files here, or click to browse
                                                </p>
                                                <input
                                                    type="file"
                                                    className="file-input"
                                                    multiple
                                                    onChange={handleFileInput}
                                                />
                                            </div>
                                        </div>
                                        
                                        {/* Uploaded Files List */}
                                        {uploadedFiles.length > 0 && (
                                            <div className="uploaded-files">
                                                <p className="files-title">Selected Files:</p>
                                                {uploadedFiles.map((file, index) => (
                                                    <div key={index} className="file-item">
                                                        <icons.IoDocument className="file-icon" />
                                                        <span className="file-name">{file.name}</span>
                                                        <span className="file-size">
                                                            ({(file.size / 1024 / 1024).toFixed(2)} MB)
                                                        </span>
                                                        <button 
                                                            className="remove-file-btn"
                                                            onClick={() => removeFile(index)}
                                                        >
                                                            <icons.IoClose />
                                                        </button>
                                                    </div>
                                                ))}
                                            </div>
                                        )}
                                    </div>
                                )}
                                
                                {/* Submit Button */}
                                <div className="submit-button-container">
                                    <button 
                                        className="submit-assignment-btn"
                                        onClick={handleSubmit}
                                        disabled={
                                            (assignment.submissionType === 'git' && !gitUrl) ||
                                            (assignment.submissionType === 'file' && uploadedFiles.length === 0)
                                        }
                                    >
                                        <icons.IoSendOutline />
                                        {assignment.status === 'Pending' ? 'Submit Assignment' : 'Resubmit Assignment'}
                                    </button>
                                </div>
                            </div>
                        )}
                        
                        {/* Rubric Details - Only for Graded assignments */}
                        {assignment.status === 'Graded' && assignment.rubric && (
                            <div className="rubric-section">
                                <h4 className="rubric-title">
                                    <icons.IoDocumentTextOutline className="rubric-icon" />
                                    Grading Rubric: {assignment.rubric.name}
                                </h4>
                                
                                <div className="rubric-summary">
                                    <div className="total-marks">
                                        <span className="total-label">Total Marks:</span>
                                        <span className="total-score">{assignment.rubric.totalMarks}/100</span>
                                    </div>
                                </div>
                                
                                <div className="criteria-list">
                                    {assignment.rubric.criteria.map((criterion, index) => (
                                        <div key={index} className="criterion-item">
                                            <div className="criterion-header">
                                                <div className="criterion-info">
                                                    <h5 className="criterion-name">{criterion.name}</h5>
                                                </div>
                                                <div className="criterion-marks">
                                                    <span className="marks-awarded">{criterion.marksAwarded}</span>
                                                    <span className="marks-separator">/</span>
                                                    <span className="marks-out-of">{criterion.marksOutOf}</span>
                                                </div>
                                            </div>
                                            <div className="marks-bar">
                                                <div 
                                                    className="marks-progress"
                                                    style={{
                                                        width: `${(criterion.marksAwarded / criterion.marksOutOf) * 100}%`
                                                    }}
                                                ></div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                                
                                <div className="rubric-footer">
                                    <div className="marks-breakdown">
                                        <div className="breakdown-item">
                                            <span className="breakdown-label">Total Awarded:</span>
                                            <span className="breakdown-value">{assignment.rubric.totalMarks} marks</span>
                                        </div>
                                        <div className="breakdown-item">
                                            <span className="breakdown-label">Maximum Possible:</span>
                                            <span className="breakdown-value">100 marks</span>
                                        </div>
                                        <div className="breakdown-item">
                                            <span className="breakdown-label">Percentage:</span>
                                            <span className="breakdown-value percentage">
                                                {assignment.rubric.totalMarks}%
                                            </span>
                                        </div>
                                    </div>
                                </div>

                                {/* Supervisor Comments - Simple version */}
                                {assignment.rubric.supervisorComment && (
                                    <div className="supervisor-comments">
                                        <p>{assignment.rubric.supervisorComment}</p>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>
                )}
                <div className="assignment-footer">
                    <div className="due-date">
                        <icons.IoTimeOutline/>
                        Due: {assignment.dueDate}
                    </div>
                </div>
            </div>
        );
    };

    return (
        <>
            <h1 style={{ fontFamily: 'arial', fontSize: '40px', marginLeft:'20px'}}> Reports & Assignments</h1>
            <p style={{ fontFamily: 'arial', fontSize:'20px', marginLeft:'20px', color: '#3d3d3d'}}>View learning & assignments tasks</p>
            
            <div className="category-row">
                <button 
                    className={filter === 'all' ? 'active all' : ''}
                    onClick={() => setFilter('all')}
                >
                    All assignments
                </button>
                <button 
                    className={filter === 'pending' ? 'active pending' : ''}
                    onClick={() => setFilter('pending')}
                >
                    Pending
                </button>
                <button 
                    className={filter === 'submitted' ? 'active submitted' : ''}
                    onClick={() => setFilter('submitted')}
                >
                    Submitted
                </button>
                <button 
                    className={filter === 'graded' ? 'active graded' : ''}
                    onClick={() => setFilter('graded')}
                >
                    Graded
                </button>
            </div>
            
            <div className='content-area'>
                {filteredAssignments.length > 0 ? (
                    filteredAssignments.map(assignment => (
                        <AssignmentCard 
                            key={assignment.id}
                            assignment={assignment}
                        />
                    ))
                ) : (
                    <div className="no-assignments">
                        No assignments found for this category.
                    </div>
                )}
            </div>
        </>
    );
}

export default StudentAssignment;