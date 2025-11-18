import { useState, useRef, useEffect } from "react";
import '../Styles/reports.css';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { TextField } from '@mui/material';
import { IoAlertCircleOutline, IoCheckmarkCircleOutline, IoClipboardOutline, IoAdd, IoClose, IoTrash, IoPersonAdd, IoChevronDown, IoChevronUp, IoDownloadOutline, IoOpenOutline } from "react-icons/io5";
import { rubricAPI, submissionAPI, assignmentAPI, usersAPI } from "../services/api";
import {motion} from 'framer-motion'


const UngradedSubmissionCard = ({ submission, onMarksUpdate, onCommentUpdate, onSubmitGrade }) => {
    const [isExpanded, setIsExpanded] = useState(false);
    const [comment, setComment] = useState(submission.supervisorComment || '');
    const commentTextareaRef = useRef(null);

   
    const calculateTotalMarks = (submission) => {
        return submission.rubric.criteria.reduce((total, criterion) => total + criterion.awardedMarks, 0);
    };

    
    const calculateMaxMarks = (submission) => {
        return submission.rubric.criteria.reduce((total, criterion) => total + criterion.maxMarks, 0);
    };

    const totalMarks = calculateTotalMarks(submission);
    const maxMarks = calculateMaxMarks(submission);

    const toggleExpand = () => {
        setIsExpanded(!isExpanded);
    };

    
    useEffect(() => {
        const textarea = commentTextareaRef.current;
        if (textarea) {
            textarea.style.height = 'auto';
            textarea.style.height = `${textarea.scrollHeight}px`;
        }
    }, [comment]);

    const handleCommentChange = (e) => {
        const newComment = e.target.value;
        setComment(newComment);
        onCommentUpdate(submission.id, newComment);
    };

   
    const openGitUrl = (url) => {
        window.open(url, '_blank');
    };

   
    const handleDownload = (fileName) => {
        
        alert(`Downloading file: ${fileName}`);
        
    };

    const handleSubmitGrade = () => {
        onSubmitGrade(submission.id);
    };

    return (
        <div className={`sup-submission-card ${isExpanded ? 'sup-expanded' : ''}`}>
            <div className="sup-submission-header">
                <div className="sup-submission-title-section">
                    <h3 className="sup-submission-title">{submission.assignmentTitle}</h3>
                    <div className="sup-submission-meta">
                        <div className="sup-student-info">
                            <span>Submitted by: {submission.studentName}</span>
                        </div>
                        <div className="sup-submission-date">
                            <span>Submitted: {submission.submittedDate}</span>
                        </div>
                    </div>
                </div>
                <div className="sup-submission-status">
                    <span className={`sup-status sup-${submission.status.toLowerCase()}`}>
                        {submission.status}
                    </span>
                </div>
                <button 
                    className="sup-view-details-btn"
                    onClick={toggleExpand}
                >
                    {isExpanded ? 'View Less' : 'View Details'}
                    {isExpanded ? <IoChevronUp /> : <IoChevronDown />}
                </button>
            </div>
            
            {isExpanded && (
                <div className="sup-submission-expanded-content">
                    {/* Git Repository Submission */}
                    {submission.submissionType === 'git' && (
                        <div className="sup-git-submission-details">
                            <div className="sup-submission-info">
                                <strong>Git Repository URL:</strong>
                                <div className="sup-url-container">
                                    <span className="sup-git-url">{submission.gitUrl}</span>
                                    <button 
                                        className="sup-open-url-btn"
                                        onClick={() => openGitUrl(submission.gitUrl)}
                                    >
                                        <IoOpenOutline />
                                        Open Repository
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}
                    
                    {/* File Upload Submission */}
                    {submission.submissionType === 'file' && (
                        <div className="sup-file-submission-details">
                            <div className="sup-submission-info">
                                <strong>Submitted File:</strong>
                                <div className="sup-file-container">
                                    <div className="sup-file-info">
                                        <span className="sup-file-name">{submission.fileName}</span>
                                        <span className="sup-file-size">{submission.fileSize}</span>
                                    </div>
                                    <button 
                                        className="sup-download-file-btn"
                                        onClick={() => handleDownload(submission.fileName)}
                                    >
                                        <IoDownloadOutline />
                                        Download File
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}
                    
                    {/* Grading Rubric Section */}
                    <div className="sup-grading-rubric-section">
                        <h4 className="sup-rubric-title">{submission.rubric.name}</h4>
                        
                        <div className="sup-criteria-list">
                            {submission.rubric.items.map((criterion) => (
                                <div key={criterion.id} className="sup-criterion-item">
                                    <div className="sup-criterion-info">
                                        <div className="sup-criterion-header">
                                            <h5 className="sup-criterion-name">{criterion.name}</h5>
                                            <div className="sup-criterion-marks-display">
                                                <span className="sup-awarded-marks">{criterion.awardedMarks}</span>
                                                <span className="sup-marks-separator">/</span>
                                                <span className="sup-max-marks">{criterion.maxMarks}</span>
                                            </div>
                                        </div>
                                        <p className="sup-criterion-description">{criterion.description}</p>
                                    </div>
                                    <div className="sup-marks-input-container">
                                        <label className="sup-marks-label">Marks Awarded:</label>
                                        <input
                                            type="number"
                                            min="0"
                                            max={criterion.maxMarks}
                                            value={criterion.awardedMarks}
                                            onChange={(e) => onMarksUpdate(submission.id, criterion.id, e.target.value)}
                                            className="sup-marks-input"
                                        />
                                        {criterion.awardedMarks > criterion.maxMarks && (
                                            <div className="marks-error">
                                                Cannot exceed {criterion.maxMarks} marks
                                            </div>
                                        )}
                                    </div>
                                </div>
                            ))}
                        </div>
                        
                        <div className="sup-total-marks-section">
                            <div className="sup-total-marks-display">
                                <span className="sup-total-label">Total Marks:</span>
                                <span className={`sup-total-score ${totalMarks > maxMarks ? 'sup-total-error' : ''}`}>
                                    {totalMarks}/{maxMarks}
                                </span>
                            </div>
                            <div className="sup-percentage-display">
                                <span className="sup-percentage-label">Percentage:</span>
                                <span className="sup-percentage-value">
                                    {maxMarks > 0 ? Math.round((totalMarks / maxMarks) * 100) : 0}%
                                </span>
                            </div>
                            {totalMarks > maxMarks && (
                                <div className="total-marks-error">
                                    Total marks cannot exceed {maxMarks}
                                </div>
                            )}
                        </div>

                        {/* Supervisor Comments Section */}
                        <div className="sup-comments-section">
                            <h5 className="sup-comments-title">Supervisor Comments</h5>
                            <div className="sup-comments-container">
                                <textarea
                                    ref={commentTextareaRef}
                                    value={comment}
                                    onChange={handleCommentChange}
                                    placeholder="Provide feedback and comments for the student..."
                                    className="sup-comments-textarea"
                                    rows={3}
                                />
                                <div className="sup-comments-help">
                                    <span>Provide constructive feedback about the student's work, strengths, and areas for improvement.</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div className="sup-submission-actions">
                        <button 
                            className="sup-submit-grade-btn"
                            onClick={handleSubmitGrade}
                            disabled={totalMarks > maxMarks}
                        >
                            Submit Grade
                        </button>
                        {totalMarks > maxMarks && (
                            <div className="submit-error">
                                Please adjust marks to not exceed maximum available marks
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};


const GradedSubmissionCard = ({ submission }) => {
    const [isExpanded, setIsExpanded] = useState(false);

    
    const calculateTotalMarks = (submission) => {
        return submission.rubric.criteria.reduce((total, criterion) => total + criterion.awardedMarks, 0);
    };

    
    const calculateMaxMarks = (submission) => {
        return submission.rubric.criteria.reduce((total, criterion) => total + criterion.maxMarks, 0);
    };

    const totalMarks = calculateTotalMarks(submission);
    const maxMarks = calculateMaxMarks(submission);
    const percentage = maxMarks > 0 ? Math.round((totalMarks / maxMarks) * 100) : 0;

    const toggleExpand = () => {
        setIsExpanded(!isExpanded);
    };

   
    const openGitUrl = (url) => {
        window.open(url, '_blank');
    };

  
    const handleDownload = (fileName) => {
        alert(`Downloading file: ${fileName}`);
    };

    return (
        <div className={`sup-submission-card ${isExpanded ? 'sup-expanded' : ''}`}>
            <div className="sup-submission-header">
                <div className="sup-submission-title-section">
                    <h3 className="sup-submission-title">{submission.assignmentTitle}</h3>
                    <div className="sup-submission-meta">
                        <div className="sup-student-info">
                            <span>Submitted by: {submission.studentName}</span>
                        </div>
                        <div className="sup-submission-date">
                            <span>Submitted: {submission.submittedDate}</span>
                        </div>
                        <div className="sup-grade-info">
                            <span className="sup-total-grade">Grade: {totalMarks}/{maxMarks} ({percentage}%)</span>
                        </div>
                    </div>
                </div>
                <div className="sup-submission-status">
                    <span className="sup-status sup-graded">
                        Graded
                    </span>
                </div>
                <button 
                    className="sup-view-details-btn"
                    onClick={toggleExpand}
                >
                    {isExpanded ? 'View Less' : 'View Details'}
                    {isExpanded ? <IoChevronUp /> : <IoChevronDown />}
                </button>
            </div>
            
            {isExpanded && (
                <div className="sup-submission-expanded-content">
                    {/* Git Repository Submission */}
                    {submission.submissionType === 'git' && (
                        <div className="sup-git-submission-details">
                            <div className="sup-submission-info">
                                <strong>Git Repository URL:</strong>
                                <div className="sup-url-container">
                                    <span className="sup-git-url">{submission.gitUrl}</span>
                                    <button 
                                        className="sup-open-url-btn"
                                        onClick={() => openGitUrl(submission.gitUrl)}
                                    >
                                        <IoOpenOutline />
                                        Open Repository
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}
                    
                    {/* File Upload Submission */}
                    {submission.submissionType === 'file' && (
                        <div className="sup-file-submission-details">
                            <div className="sup-submission-info">
                                <strong>Submitted File:</strong>
                                <div className="sup-file-container">
                                    <div className="sup-file-info">
                                        <span className="sup-file-name">{submission.fileName}</span>
                                        <span className="sup-file-size">{submission.fileSize}</span>
                                    </div>
                                    <button 
                                        className="sup-download-file-btn"
                                        onClick={() => handleDownload(submission.fileName)}
                                    >
                                        <IoDownloadOutline />
                                        Download File
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}
                    
                    {/* Grading Rubric Section */}
                    <div className="sup-grading-rubric-section">
                        <h4 className="sup-rubric-title">{submission.rubric.name}</h4>
                        
                        <div className="sup-criteria-list">
                            {submission.rubric.criteria.map((criterion) => (
                                <div key={criterion.id} className="sup-criterion-item">
                                    <div className="sup-criterion-info">
                                        <div className="sup-criterion-header">
                                            <h5 className="sup-criterion-name">{criterion.name}</h5>
                                            <div className="sup-criterion-marks-display">
                                                <span className="sup-awarded-marks">{criterion.awardedMarks}</span>
                                                <span className="sup-marks-separator">/</span>
                                                <span className="sup-max-marks">{criterion.maxMarks}</span>
                                            </div>
                                        </div>
                                        <p className="sup-criterion-description">{criterion.description}</p>
                                    </div>
                                </div>
                            ))}
                        </div>
                        
                        <div className="sup-total-marks-section">
                            <div className="sup-total-marks-display">
                                <span className="sup-total-label">Total Marks:</span>
                                <span className="sup-total-score">{totalMarks}/{maxMarks}</span>
                            </div>
                            <div className="sup-percentage-display">
                                <span className="sup-percentage-label">Percentage:</span>
                                <span className="sup-percentage-value">
                                    {percentage}%
                                </span>
                            </div>
                        </div>

                        {/* Supervisor Comments Section */}
                        {submission.supervisorComment && (
                            <div className="sup-comments-section">
                                <h5 className="sup-comments-title">Supervisor Comments</h5>
                                <div className="sup-comments-container">
                                    <div className="sup-comments-text">
                                        {submission.supervisorComment}
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};

function SupervisorReports(){
    const [isCreateAsssignmentOpen, setIsCreateAssignmentOpen] = useState(false);
    const [isGradingRubricOpen, setIsGradingRubricOpen] = useState(false);
    const [isRubricListOpen, setIsRubricListOpen] = useState(false);
    const [assignmentDescription, setAssignmentDescription] = useState('');
    const [selectedInterns, setSelectedInterns] = useState([]);
    const [interns, setInterns] = useState([]);
    const [criteria, setCriteria] = useState([]);
    const [criterionDescription, setCriterionDescription] = useState('');
    const [criterionName, setCriterionName] = useState('');
    const [totalMarks, setTotalMarks] = useState(0);
    const [rubricName, setRubricName] = useState('');
    const [rubricDescription, setRubricDescription] = useState('');
    const [rubrics, setRubrics] = useState([]);
    const [selectedRubric, setSelectedRubric] = useState(0);
    const [assignmentTitle, setAssignmentTitle] = useState('');
    const [submissionType, setSubmissionType] = useState(0);
    const [assignments, setAssignments] = useState([]);
    const [createdAssignments, setCreatedAssignments] = useState([]);
    const [ongoingAssignments, setOngoingAssignments] = useState([]);
    const textareaRef = useRef(null);
    const [dueDate, setDueDate] = useState(null);
    const [assignToInterns, setAssigntointerns] = useState(false);
    const [currentAssignmentId, setCurrentAssignmentId] = useState(null);
    const [ungradedSubmissions, setUngradedSubmissions] = useState([]);

    
    const [gradedSubmissions, setGradedSubmissions] = useState([]);

    const fetchRubrics  = async() => {
        const response = await rubricAPI.getAllRubrics();
        setRubrics(response.data);
    }

    const fetchStudents = async () => {
        const response = await usersAPI.getUsers();
        const users = response.data;
        // const studentRoles = ['attache','intern'];
        // const studentUsers = users.filter(user => {return studentRoles.includes(user.role)});
        setInterns(users);       

    }

    const fetchAssignments =  async () => {
        const response = await assignmentAPI.getAllAssignments();
        const assignments = response.data;
        setCreatedAssignments(assignments);
        // const userDepartmentId = '';
        // const departmentAssignments = assignments.filter(assignment => assignment.departnmentId === userDepartmentId)
    }
    
    useEffect(() => {
        const textarea = textareaRef.current;
        if (textarea) {
            textarea.style.height = 'auto';
            textarea.style.height = `${textarea.scrollHeight}px`;
        }
    }, [assignmentDescription]);

    
    useEffect(() => {
        const total = criteria.reduce((sum, criterion) => sum + criterion.maxScore, 0);
        setTotalMarks(total);
    }, [criteria]);

    useEffect(() => {
        fetchRubrics();
        fetchStudents();
        fetchAssignments();

    }, []);
   
    const handleCheckboxChange = (internId) => {
        setSelectedInterns(prev => {
            if (prev.includes(internId)) {
                return prev.filter(id => id !== internId);
            } else {
                return [...prev, internId];
            }
        });
    };

    
    const handleSelectAll = () => {
        if (selectedInterns.length === interns.length) {
            setSelectedInterns([]);
        } else {
            setSelectedInterns(interns.map(intern => intern.id));
        }
    };

   
    const addCriterion = () => {
        const availableMarks = 100 - totalMarks;
        const defaultMarks = availableMarks > 0 ? 1 : 0;
        
        setCriteria(prev => [
            ...prev,
            {
                id: Date.now().toString(),
                criteria: criterionName || '',
                description: criterionDescription || '',
                maxScore: defaultMarks,
                order: 1
            }
        ]);

        setCriterionName('');
        setCriterionDescription('');
    };

    
    const updateCriterionName = (id, name) => {
        setCriteria(prev => prev.map(criterion => 
            criterion.id === id ? { ...criterion, criteria:name } : criterion
        ));
    };

    
    const updateCriterionMarks = (id, newMarks) => {
        const numericMarks = parseInt(newMarks) || 0;
        const currentCriterion = criteria.find(c => c.id === id);
        const currentMarks = currentCriterion ? currentCriterion.maxScore : 0;
        const marksDifference = numericMarks - currentMarks;
        const newTotal = totalMarks + marksDifference;

        if (newTotal <= 100 && numericMarks >= 0 && numericMarks <= 100) {
            setCriteria(prev => prev.map(criterion => 
                criterion.id === id ? { ...criterion, maxScore: numericMarks } : criterion
            ));
        }
    };

    
    const removeCriterion = (id) => {
        setCriteria(prev => prev.filter(criterion => criterion.id !== id));
    };

    // Distribute remaining marks evenly
    const distributeRemainingMarks = () => {
        const remainingMarks = 100 - totalMarks;
        if (remainingMarks > 0 && criteria.length > 0) {
            const marksPerCriterion = Math.floor(remainingMarks / criteria.length);
            const extraMarks = remainingMarks % criteria.length;
            
            setCriteria(prev => prev.map((criterion, index) => ({
                ...criterion,
                marks: criterion.marks + marksPerCriterion + (index < extraMarks ? 1 : 0)
            })));
        }
    };

    
    const createRubric = async () => {
        if (rubricName.trim() && totalMarks === 100 && criteria.length > 0) {
            const newRubric = {
                
                name: rubricName,
                description: rubricDescription,
                items: [...criteria]
                
            };
            
            
            console.log('new rubric: ', newRubric);
            await rubricAPI.createRubric(newRubric);
            // Reset form
            setRubricName('');
            setCriteria([]);
            setTotalMarks(0);
            setIsGradingRubricOpen(false);
            fetchRubrics();
        }
    };

    const deleteRubric = async (rubricId) => {
        setRubrics(prev => prev.filter(rubric => rubric.id !== rubricId));
        // await rubricAPI.deleteRubric(rubricId); waiting for Wayne to update delete endpoint
        if (selectedRubric === rubricId.toString()) {
            setSelectedRubric(0);
        }
    };

   
    const closeRubricCreation = () => {
        setIsGradingRubricOpen(false);
        setRubricName('');
        setCriteria([]);
        setTotalMarks(0);
    };

  
    const createAssignment = async () => {
        if (assignmentTitle.trim() && dueDate) {
           
            const selectedRubricObj = rubrics.find(r => r.id.toString() === selectedRubric);
            
            const newAssignment = {
                
                title: assignmentTitle,
                description: assignmentDescription,
                dueDate: dueDate,
                departmentId: 1,
                allowedSubmissionType: submissionType,
                rubricId: selectedRubric || null,
            };
            await assignmentAPI.createAssignment(newAssignment);
            
            setCreatedAssignments(prev => [...prev, newAssignment]);
            
            // Reset form and close modal
            setAssignmentTitle('');
            setAssignmentDescription('');
            setDueDate(null);
            setSelectedRubric(0);
            setSubmissionType(0);
            setSelectedInterns([]);
            setIsCreateAssignmentOpen(false);
        }
    };

   
    const openAssignToModal = (assignmentId) => {
        setCurrentAssignmentId(assignmentId);
        setAssigntointerns(true);
        setSelectedInterns([]);
    };

    
    const assignToStudents = async () => {
        if (currentAssignmentId && selectedInterns.length > 0) {
          
            const updatedAssignment = createdAssignments.find(assignment => assignment.id === currentAssignmentId);
            if (updatedAssignment) {
                const assignedAssignment = {
                    ...updatedAssignment,
                    assignedTo: [...selectedInterns],
                   
                };

                const userIds = {
                    userIds: selectedInterns
                }

                await assignmentAPI.assignAssignment(currentAssignmentId, userIds);
                setCreatedAssignments(prev => prev.filter(assignment => assignment.id !== currentAssignmentId));
                
               
                setOngoingAssignments(prev => [...prev, assignedAssignment]);
            }

           
            setAssigntointerns(false);
            setCurrentAssignmentId(null);
            setSelectedInterns([]);
        }
    };

   
    const handleMarksUpdate = (submissionId, criterionId, marks) => {
        const numericMarks = parseInt(marks) || 0;
        
        setUngradedSubmissions(prev => prev.map(submission => {
            if (submission.id === submissionId) {
                const criterion = submission.rubric.criteria.find(c => c.id === criterionId);
                if (criterion) {
                   
                    const finalMarks = Math.min(numericMarks, criterion.maxMarks);
                    
                    const updatedRubric = {
                        ...submission.rubric,
                        criteria: submission.rubric.criteria.map(criterion => 
                            criterion.id === criterionId 
                                ? { ...criterion, awardedMarks: finalMarks }
                                : criterion
                        )
                    };
                    return { ...submission, rubric: updatedRubric };
                }
            }
            return submission;
        }));
    };

    
    const handleCommentUpdate = (submissionId, comment) => {
        setUngradedSubmissions(prev => prev.map(submission => 
            submission.id === submissionId 
                ? { ...submission, supervisorComment: comment }
                : submission
        ));
    };

    
    const handleSubmitGrade = (submissionId) => {
        
        const submissionToGrade = ungradedSubmissions.find(sub => sub.id === submissionId);
        
        if (submissionToGrade) {
           
            const hasExceededMarks = submissionToGrade.rubric.criteria.some(
                criterion => criterion.awardedMarks > criterion.maxMarks
            );
            
            if (hasExceededMarks) {
                alert('Cannot submit grade: Some criteria have marks exceeding maximum allowed values.');
                return;
            }

           
            const gradedSubmission = {
                ...submissionToGrade,
                gradedDate: new Date().toLocaleDateString()
            };

            
            setGradedSubmissions(prev => [...prev, gradedSubmission]);
            
          
            setUngradedSubmissions(prev => prev.filter(sub => sub.id !== submissionId));
            
            alert(`Grade submitted for ${submissionToGrade.assignmentTitle} by ${submissionToGrade.studentName}`);
        }
    };

    return(
         <motion.div
  initial={{ x: -100, opacity: 0 }}
  animate={{ x: 0, opacity: 1 }}
  transition={{ 
    duration: 1.7,
    ease: [0.25, 0.46, 0.45, 0.94] 
  }}
>
        <LocalizationProvider dateAdapter={AdapterDateFns}>
            <div>
                <h1 style={{ fontFamily:"arial", fontSize: " 35px", marginLeft: "20px"}}> Assignments & Reports</h1>
                <p style={{ fontFamily: 'arial', fontSize:'20px', marginLeft:'20px', color: '#3d3d3d'}}> Create assignments and assign them to Attachés or interns in your department</p>
                <button 
                className="create-assn-button"
                onClick={ () => setIsCreateAssignmentOpen (true)}
                >
                    New Assignment
                </button>
                <button 
                className="create-assn-button"
                onClick={ () => setIsGradingRubricOpen (true)}
                >
                    New Grading rubric
                </button>
                 <button 
                className="create-assn-button"
                onClick={ () => setIsRubricListOpen (true)}
                >
                    My Rubrics ({rubrics.length})
                </button>

                
                <div className="report-stats-row" >
                    <div className="report-stats-container">
                              <div className="icon-container">
                                <IoClipboardOutline style={{ fontSize: "40px" }} />
                              </div>
                              <div className="content-wrapper">
                                <div className="label">Ongoing</div>
                                <div className="number">9</div>
                              </div>
                            </div>

                            <div className="report-stats-container">
                              <div className="icon-container">
                                <IoCheckmarkCircleOutline style={{ fontSize: "40px" }} />
                              </div>
                              <div className="content-wrapper">
                                <div className="label">Complete graded</div>
                                <div className="number">{gradedSubmissions.length}</div>
                              </div>
                            </div>

                            <div className="report-stats-container">
                              <div className="icon-container">
                                <IoAlertCircleOutline style={{ fontSize: "40px" }} />
                              </div>
                              <div className="content-wrapper">
                                <div className="label">Complete ungraded</div>
                                <div className="number">{ungradedSubmissions.length}</div>
                              </div>
                            </div>

                </div>
                
                <h1 style={{ fontFamily:"arial", fontSize: " 30px", marginLeft: "20px"}}> Created Assignments </h1>
                <div className="created-assignments-section">
                    {createdAssignments.length === 0 ? (
                        <div className="empty-assignments">
                            <p>No assignments created yet</p>
                        </div>
                    ) : (
                        <div className="assignments-grid">
                            {createdAssignments.map(assignment => (
                                <div key={assignment.id} className="assignment-container">
                                    <div className="assignment-header">
                                        <h3 className="assignment-title">{assignment.title}</h3>
                                        <span className={`assignment-status ${assignment.status?.toLowerCase() || ''} `}>
                                            {assignment.status}
                                        </span>
                                    </div>
                                    <div className="assignment-details">
                                        <div className="assignment-description">
                                            <strong>Description:</strong>
                                            <p>{assignment.description || "No description provided"}</p>
                                        </div>
                                        <div className="assignment-meta">
                                            <div className="meta-item">
                                                <strong>Due Date:</strong>
                                                <span>{assignment.dueAt ? new Date(assignment.dueAt).toLocaleDateString() : "Not set"}</span>
                                            </div>
                                            <div className="meta-item">
                                                <strong>Submission:</strong>
                                                <span>{assignment.submissionType || ''}</span>
                                            </div>
                                            <div className="meta-item">
                                                <strong>Rubric:</strong>
                                                <span>{assignment.rubric ? assignment.rubric.name : "None"}</span>
                                            </div>
                                            <div className="meta-item">
                                                <strong>Assigned to:</strong>
                                                <span>
                                                    {assignment.assigneeCount} Students
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="assignment-actions">
                                        <button 
                                            className="assign-to-btn"
                                            onClick={() => openAssignToModal(assignment.id)}
                                            disabled={assignment.assigneeCount > 0}
                                        >
                                            <IoPersonAdd style={{ marginRight: '5px' }} />
                                            {assignment.assigneeCount > 0 ? 'Assigned' : 'Assign to students'}
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                <h1 style={{ fontFamily:"arial", fontSize: " 30px", marginLeft: "20px"}}> Ongoing Assignments </h1>
                <div className="ongoing-section">
                    {ongoingAssignments.length === 0 ? (
                        <div className="empty-assignments">
                            <p>No ongoing assignments</p>
                        </div>
                    ) : (
                        ongoingAssignments.map(assignment => (
                            <div key={assignment.id} className="report-container">
                                <h3 className="assignment-title">{assignment.title}</h3>
                                <span className="assignment-status">{assignment.status}</span>
                            
                                <div className="assignment-details">
                                    <div className="assigned-users">
                                        <strong>Assigned to: </strong>
                                        {assignment.assignedTo?.map((user, index) => (
                                            <span key={user} className="user-tag">
                                                {user}
                                                {index < assignment.assignedTo.length - 1 && ', '}
                                            </span>
                                        )) || ''}
                                    </div>
                                    <div className="due-date">
                                        <strong>Due: </strong>
                                        {new Date(assignment.dueAt).toLocaleDateString()}
                                    </div>
                                    <div className="assignment-description">
                                        <strong>Description: </strong>
                                        {assignment.description || "No description provided"}
                                    </div>
                                    <div className="submission-type">
                                        <strong>Submission: </strong>
                                        {assignment.submissionType || ''}
                                    </div>
                                    <div className="rubric-info">
                                        <strong>Rubric: </strong>
                                        {assignment.rubric ? assignment.rubric.name : "None"}
                                    </div>
                                </div>
                            </div> 
                        ))
                    )}
                </div>

                <h1 style={{ fontFamily:"arial", fontSize: " 30px", marginLeft: "20px"}}> Submitted Assignments </h1>
                <h2  style={{ fontFamily:"arial", fontSize: " 20px", marginLeft: "20px"}}> Ungraded</h2>
                <div className="sup-ungraded-submissions-section">
                    {ungradedSubmissions.length === 0 ? (
                        <div className="sup-empty-submissions">
                            <p>No ungraded submissions</p>
                        </div>
                    ) : (
                        <div className="sup-submissions-list">
                            {ungradedSubmissions.map(submission => (
                                <UngradedSubmissionCard 
                                    key={submission.id}
                                    submission={submission}
                                    onMarksUpdate={handleMarksUpdate}
                                    onCommentUpdate={handleCommentUpdate}
                                    onSubmitGrade={handleSubmitGrade}
                                />
                            ))}
                        </div>
                    )}
                </div>
                
                <h2  style={{ fontFamily:"arial", fontSize: " 20px", marginLeft: "20px"}}> Graded</h2>
                <div className="sup-ungraded-submissions-section">
                    {gradedSubmissions.length === 0 ? (
                        <div className="sup-empty-submissions">
                            <p>No graded submissions</p>
                        </div>
                    ) : (
                        <div className="sup-submissions-list">
                            {gradedSubmissions.map(submission => (
                                <GradedSubmissionCard 
                                    key={submission.id}
                                    submission={submission}
                                />
                            ))}
                        </div>
                    )}
                </div>



                {/* MODALS */}

                {/* CREATE NEW ASSIGNMENT */}
                { isCreateAsssignmentOpen && (
                    <div className="modal-overlay">
                        <div className="rev-modal-container">
    <div className="rev-modal-header">
        <h2 className="rev-modal-title">
            Create new assignment
        </h2>
    </div>
    
    <div className="rev-modal-content">
        <div className="rev-form-group">
            <label className="rev-form-label">Assignment Title *</label>
            <input
                type="text"
                className="rev-form-input"
                placeholder="Enter assignment title"
                value={assignmentTitle}
                onChange={(e) => setAssignmentTitle(e.target.value)}
            />
        </div>
        
        <div className="rev-form-group">
            <label className="rev-form-label">Assignment Description</label>
            <textarea
                ref={textareaRef}
                className="rev-form-textarea"
                value={assignmentDescription}
                onChange={(e) => setAssignmentDescription(e.target.value)}
                placeholder="Enter assignment description..."
                rows={3}
            />
        </div>

        <div className="rev-form-group">
            <label className="rev-form-label">Due Date *</label>
            <DatePicker
                value={dueDate}
                onChange={(newDate) => setDueDate(newDate)}
                renderInput={(params) => (
                    <TextField 
                        {...params} 
                        fullWidth
                        className="rev-form-input"
                        placeholder="Click to select due date"
                    />
                )}
            />
        </div>

        <div className="rev-form-group">
            <label className="rev-form-label">Submission Type</label>
            <select 
                value={submissionType} 
                onChange={(e) => setSubmissionType(e.target.value)}
                className="rev-form-select"
            >
                <option value={1}>Git Repository</option>
                <option value={2}>File Upload</option>
            </select>
            <div className="rev-selected-students">
                {submissionType === '1' && (
                    <span>Students will submit Git repository URLs</span>
                )}
                {submissionType === '2' && (
                    <span>Students will upload files (PDF, DOC, ZIP, etc.)</span>
                )}
            </div>
        </div>
        
        <div className="rev-form-group">
            <label className="rev-form-label">Grading Rubric</label>
            <select 
                value={selectedRubric} 
                onChange={(e) => setSelectedRubric(e.target.value)}
                className="rev-form-select"
            >
                <option value="">Select a grading rubric</option>
                {rubrics.map(rubric => (
                    <option key={rubric.id} value={rubric.id}>
                        {rubric.name} ({rubric.items.length} criteria)
                    </option>
                ))}
            </select>
            
            {selectedRubric && (
                <div className="selected-rubric-info">
                    <div className="rev-selected-students">
                        <strong>Selected Rubric: </strong>
                        {rubrics.find(r => r.id.toString() === selectedRubric)?.name}
                    </div>
                    <div className="rubric-preview">
                        {rubrics.find(r => r.id.toString() === selectedRubric)?.items.map((criterion, index) => (
                            <div key={criterion.id} className="preview-criterion-small">
                                <span>{criterion.criteria}</span>
                                <span>{criterion.maxScore} pts</span>
                            </div>
                        ))}
                    </div>
                </div>
            )}
            
            <div className="rubric-actions" style={{ display: 'flex', gap: '12px', marginTop: '12px' }}>
                <button 
                    type="button"
                    className="rev-cancel-btn"
                    style={{ flex: 1, fontSize: '14px', padding: '10px 16px' }}
                    onClick={() => {
                        setIsCreateAssignmentOpen(false);
                        setIsGradingRubricOpen(true);
                    }}
                >
                    Create New Rubric
                </button>
                <button 
                    type="button"
                    className="rev-cancel-btn"
                    style={{ flex: 1, fontSize: '14px', padding: '10px 16px' }}
                    onClick={() => {
                        setIsCreateAssignmentOpen(false);
                        setIsRubricListOpen(true);
                    }}
                >
                    View All Rubrics
                </button>
            </div>
        </div>
    </div>

    <div className="rev-modal-actions">
        <button 
            className="rev-cancel-btn"
            onClick={() => setIsCreateAssignmentOpen(false)}
        >
            Cancel
        </button>
        <button 
            className="rev-submit-btn"
            onClick={createAssignment}
            disabled={!assignmentTitle.trim() || !dueDate}
        >
            Create Assignment
        </button>
    </div>
</div>









                    </div>
                )}

                {/* CREATE NEW GRADING RUBRIC */}
                {isGradingRubricOpen && (
                    <div className="modal-overlay">
                        <div className="rubric-container">
                            <h2 style={{ fontFamily: "arial", marginLeft: "20px" }}>Create Grading Rubric</h2>
                            <div className="form-group">
                                <label> Rubric Name</label>
                                <input
                                    type="text"
                                    placeholder="Name your new rubric"
                                    value={rubricName}
                                    onChange={(e) => setRubricName(e.target.value)}
                                    className="rubric-name-input"
                                />
                            </div>
                            
                            {/* Total Marks Display */}
                            <div className="total-marks-display">
                                <span>Total Marks: {totalMarks}/100</span>
                                {totalMarks < 100 && (
                                    <button 
                                        className="distribute-btn"
                                        onClick={distributeRemainingMarks}
                                    >
                                        Distribute Remaining {100 - totalMarks} Marks
                                    </button>
                                )}
                            </div>

                            {/* Criteria List */}
                            <div className="criteria-list">
                                {criteria.map((criterion, index) => (
                                    
                                    <div key={criterion.id} className="criterion-item">
                                        <div className="criterion-header">
                                            <span className="criterion-number">Criterion {index + 1}</span>
                                            <button 
                                                className="remove-criterion-btn"
                                                onClick={() => removeCriterion(criterion.id)}
                                            >
                                                <IoClose />
                                            </button>
                                        </div>
                                        <div className="criterion-inputs">
                                            <input
                                                type="text"
                                                placeholder="Criterion name"
                                                value={criterion.criteria}
                                                onChange={(e) => updateCriterionName(criterion.id, e.target.value)}
                                                className="criterion-name-input"
                                            />
                                            <div className="marks-input-container">
                                                <label> Marks:</label>
                                                <input
                                                    type="number"
                                                    min="0"
                                                    max="100"
                                                    value={criterion.maxScore}
                                                    onChange={(e) => updateCriterionMarks(criterion.id, e.target.value)}
                                                    className="marks-input"
                                                />
                                                <span className="marks-out-of">/ 100</span>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>

                            {/* New Criterion Button */}
                            <button 
                                className="new-criterion-btn"
                                onClick={addCriterion}
                                disabled={totalMarks >= 100}
                            >
                                <IoAdd style={{ marginRight: '8px' }} />
                                New Criterion
                            </button>

                            <div className="modal-actions">
                                <button 
                                    className="cancel-btn" 
                                    onClick={closeRubricCreation}
                                >
                                    Cancel
                                </button>
                                <button 
                                    className="assign-btn"
                                    onClick={createRubric}
                                    disabled={totalMarks !== 100 || criteria.length === 0 || !rubricName.trim()}
                                >
                                    Create Rubric
                                </button>
                            </div>
                        </div>
                    </div>
                )}

                {/* LIST OF RUBRICS */}
                { isRubricListOpen && (
                    <div className="modal-overlay">
                        <div className="rubric-list-container">
                            <h2 style={{ fontFamily: "arial", marginLeft: "20px" }}>My Rubrics</h2>
                            
                            {rubrics.length === 0 ? (
                                <div className="empty-rubrics">
                                    <p>No rubrics created yet.</p>
                                    <button 
                                        className="create-assn-button"
                                        onClick={() => {
                                            setIsRubricListOpen(false);
                                            setIsGradingRubricOpen(true);
                                        }}
                                    >
                                        Create Your First Rubric
                                    </button>
                                </div>
                            ) : (
                                <div className="rubrics-grid">
                                    {rubrics.map(rubric => (
                                        <div key={rubric.id} className="rubric-card">
                                            <div className="rubric-card-header">
                                                <h3 className="rubric-name">{rubric.name}</h3>
                                                <button 
                                                    className="delete-rubric-btn"
                                                    onClick={() => deleteRubric(rubric.id)}
                                                >
                                                    <IoTrash />
                                                </button>
                                            </div>
                                            <div className="rubric-details">
                                                <div className="rubric-meta">
                                                    <span>Total Marks: {rubric.totalMarks || 100}</span>
                                                    <span>Criteria: {rubric.items.length}</span>
                                                    <span>Created: {rubric.createdAt}</span>
                                                </div>
                                                <div className="criteria-preview">
                                                    {rubric.items.slice(0, 3).map((criterion, index) => (
                                                        <div key={criterion.id} className="preview-criterion">
                                                            <span className="preview-name">{criterion.criteria}</span>
                                                            <span className="preview-marks">{criterion.maxScore} pts</span>
                                                        </div>
                                                    ))}
                                                    {rubric.items.length > 3 && (
                                                        <div className="more-criteria">
                                                            +{rubric.items.length - 3} more criteria
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}

                            <div className="modal-actions">
                                <button 
                                    className="cancel-btn" 
                                    onClick={() => setIsRubricListOpen(false)}
                                >
                                    Close
                                </button>
                                <button 
                                    className="create-assn-button"
                                    onClick={() => {
                                        setIsRubricListOpen(false);
                                        setIsGradingRubricOpen(true);
                                    }}
                                >
                                    Create New Rubric
                                </button>
                            </div>
                        </div>
                    </div>
                )}

                {/* LIST OF STUDENTS TO ASSIGN TO */}
                { assignToInterns && (
                    <div className="modal-overlay">
                        <div className="assign-to-container">
                            <h2 style={{ fontFamily: "arial", marginLeft: "20px" }}>Assign to Students</h2>
                            <div className="form-group">
                                <label> Select students to assign:</label>
                                <div className="checkbox-group-container">
                                    <button 
                                        type="button" 
                                        className="select-all-btn"
                                        onClick={handleSelectAll}
                                    >
                                        {selectedInterns.length === interns.length ? 'Deselect All' : 'Select All'}
                                    </button>
                                    <div className="checkbox-group">
                                        {interns.map((intern, index) => (
                                            <div key={index} className="checkbox-option">
                                                <input
                                                    key={index}
                                                    type="checkbox"
                                                    id={`intern-${index}`}
                                                    checked={selectedInterns.includes(intern.id)}
                                                    onChange={() => handleCheckboxChange(intern.id)}
                                                    className="intern-checkbox"
                                                />
                                                <label htmlFor={`intern-${index}`} className="checkbox-label">
                                                    {intern.userName}
                                                </label>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                                <div className="selected-interns">
                                    <strong style={{fontFamily: 'arial'}}>Selected ({selectedInterns.length}): </strong>
                                    {selectedInterns.length > 0 ? 
                                        selectedInterns.map(id => {
                                            const user = interns.find(intern => intern.id === id);
                                            return user ? user.userName : id;
                                        }).join(', ') 
                                        : 'None'
                                    }
                                </div>
                            </div>
                            <div className="modal-actions">
                                <button 
                                    className="cancel-btn"
                                    onClick={() => setAssigntointerns(false)}
                                > 
                                    Cancel
                                </button>
                                <button 
                                    className="assign-to-student-btn"
                                    onClick={assignToStudents}
                                    disabled={selectedInterns.length === 0}
                                >
                                    Assign to Students
                                </button>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </LocalizationProvider>
        </motion.div>
    );
}

export default SupervisorReports;