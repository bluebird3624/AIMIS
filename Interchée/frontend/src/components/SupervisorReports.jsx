import { useState, useRef, useEffect } from "react";
import '../Styles/reports.css';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { TextField } from '@mui/material';
import { IoAlertCircleOutline, IoCheckmarkCircleOutline, IoClipboardOutline, IoAdd, IoClose, IoTrash } from "react-icons/io5";

function SupervisorReports(){
    const [isCreateAsssignmentOpen, setIsCreateAssignmentOpen] = useState(false);
    const [isGradingRubricOpen, setIsGradingRubricOpen] = useState(false);
    const [isRubricListOpen, setIsRubricListOpen] = useState(false);
    const [assignmentDescription, setAssignmentDescription] = useState('');
    const [selectedInterns, setSelectedInterns] = useState([]);
    const [criteria, setCriteria] = useState([]);
    const [totalMarks, setTotalMarks] = useState(0);
    const [rubricName, setRubricName] = useState('');
    const [rubrics, setRubrics] = useState([]); // State to store created rubrics
    const [selectedRubric, setSelectedRubric] = useState(''); // State for selected rubric in assignment
    const textareaRef = useRef(null);
    const [dueDate, setDueDate] = useState(null);

    const interns = [
        'gorb',
        'rish', 
        'lecter',
        'ddddddd',
        'eeeeee',
        'fffffff'
    ];

    // Sample ongoing assignments data
    const ongoingAssignments = [
        {
            id: 1,
            title: "React Component Development",
            assignedTo: ["gorb", "rish", "lecter"],
            dueDate: "2024-02-15",
            status: "In Progress"
        },
    ];

    // Auto-resize textarea when description changes
    useEffect(() => {
        const textarea = textareaRef.current;
        if (textarea) {
            textarea.style.height = 'auto';
            textarea.style.height = `${textarea.scrollHeight}px`;
        }
    }, [assignmentDescription]);

    // Update total marks whenever criteria change
    useEffect(() => {
        const total = criteria.reduce((sum, criterion) => sum + criterion.marks, 0);
        setTotalMarks(total);
    }, [criteria]);

    // Handle checkbox change
    const handleCheckboxChange = (intern) => {
        setSelectedInterns(prev => {
            if (prev.includes(intern)) {
                return prev.filter(item => item !== intern);
            } else {
                return [...prev, intern];
            }
        });
    };

    // Select all / deselect all
    const handleSelectAll = () => {
        if (selectedInterns.length === interns.length) {
            setSelectedInterns([]);
        } else {
            setSelectedInterns([...interns]);
        }
    };

    // Add new criterion
    const addCriterion = () => {
        const availableMarks = 100 - totalMarks;
        const defaultMarks = availableMarks > 0 ? 1 : 0;
        
        setCriteria(prev => [
            ...prev,
            {
                id: Date.now(),
                name: '',
                marks: defaultMarks
            }
        ]);
    };

    // Update criterion name
    const updateCriterionName = (id, name) => {
        setCriteria(prev => prev.map(criterion => 
            criterion.id === id ? { ...criterion, name } : criterion
        ));
    };

    // Update criterion marks
    const updateCriterionMarks = (id, newMarks) => {
        const numericMarks = parseInt(newMarks) || 0;
        const currentCriterion = criteria.find(c => c.id === id);
        const currentMarks = currentCriterion ? currentCriterion.marks : 0;
        const marksDifference = numericMarks - currentMarks;
        const newTotal = totalMarks + marksDifference;

        if (newTotal <= 100 && numericMarks >= 0 && numericMarks <= 100) {
            setCriteria(prev => prev.map(criterion => 
                criterion.id === id ? { ...criterion, marks: numericMarks } : criterion
            ));
        }
    };

    // Remove criterion
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

    // Create new rubric
    const createRubric = () => {
        if (rubricName.trim() && totalMarks === 100 && criteria.length > 0) {
            const newRubric = {
                id: Date.now(),
                name: rubricName,
                criteria: [...criteria],
                totalMarks: totalMarks,
                createdAt: new Date().toLocaleDateString()
            };
            
            setRubrics(prev => [...prev, newRubric]);
            
            // Reset form
            setRubricName('');
            setCriteria([]);
            setTotalMarks(0);
            setIsGradingRubricOpen(false);
        }
    };

    // Delete rubric
    const deleteRubric = (rubricId) => {
        setRubrics(prev => prev.filter(rubric => rubric.id !== rubricId));
        // If the deleted rubric was selected in the assignment form, clear the selection
        if (selectedRubric === rubricId.toString()) {
            setSelectedRubric('');
        }
    };

    // Close rubric creation and reset form
    const closeRubricCreation = () => {
        setIsGradingRubricOpen(false);
        setRubricName('');
        setCriteria([]);
        setTotalMarks(0);
    };

    return(
        <LocalizationProvider dateAdapter={AdapterDateFns}>
            <div>
                <h1 style={{ fontFamily:"arial", fontSize: " 30px", marginLeft: "20px"}}> Assignments </h1>
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
                                <div className="number">0 </div>
                              </div>
                            </div>

                            <div className="report-stats-container">
                              <div className="icon-container">
                                <IoAlertCircleOutline style={{ fontSize: "40px" }} />
                              </div>
                              <div className="content-wrapper">
                                <div className="label">Complete ungraded</div>
                                <div className="number">0 </div>
                              </div>
                            </div>

                </div>


                <h1 style={{ fontFamily:"arial", fontSize: " 30px", marginLeft: "20px"}}> Ongoing Assignments </h1>
                <div className="ongoing-section">
                        {ongoingAssignments.map(assignment => (
                             <div   key={assignment.id} className="report-container">
                            
    
                                    <h3 className="assignment-title">{assignment.title}</h3>
                                    <span className="assignment-status">{assignment.status}</span>
                                
                                <div className="assignment-details">
                                    <div className="assigned-users">
                                        <strong>Assigned to: </strong>
                                        {assignment.assignedTo.map((user, index) => (
                                            <span key={user} className="user-tag">
                                                {user}
                                                {index < assignment.assignedTo.length - 1 && ', '}
                                            </span>
                                        ))}
                                    </div>
                                    <div className="due-date">
                                        <strong>Due: </strong>
                                        {new Date(assignment.dueDate).toLocaleDateString()}
                                    </div>
                                </div>
                           </div> 
                        ))}
                        
                </div>
                <h1 style={{ fontFamily:"arial", fontSize: " 30px", marginLeft: "20px"}}> Completed Assignments </h1>
                <h2  style={{ fontFamily:"arial", fontSize: " 20px", marginLeft: "20px"}}> Ungraded</h2>
                <div className="complete-sections">

                </div>


                {/* CREATE NEW ASSIGNMENT */}
                { isCreateAsssignmentOpen && (
                    <div className="modal-overlay">
                        <div className="assign-container">
                            <h2 style={{ fontFamily: "arial", marginLeft: "20px"}}> New assignment</h2>
                            <div className="form-group"> 
                                <label> Assignment title</label>
                                <input
                                type="text"
                                placeholder="Enter assignment title"
                                /> 
                            </div>
                            <div className="form-group">
                                <label>Assignment Description</label>
                                <textarea
                                    ref={textareaRef}
                                    value={assignmentDescription}
                                    onChange={(e) => setAssignmentDescription(e.target.value)}
                                    placeholder="Enter assignment description... "
                                    rows={1}
                                />
                            </div>
                            <div className="form-group">
                                    <label>Due Date</label>
                                    <DatePicker
                                        value={dueDate}
                                        onChange={(newDate) => setDueDate(newDate)}
                                        renderInput={(params) => (
                                            <TextField 
                                                {...params} 
                                                fullWidth
                                                placeholder="click the icon to select due date"
                                            />
                                        )}
                                        className="date-picker"
                                    />
                            </div>
                            <div className="form-group">
                                <label> Assign to :</label>
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
                                                    type="checkbox"
                                                    id={`intern-${index}`}
                                                    checked={selectedInterns.includes(intern)}
                                                    onChange={() => handleCheckboxChange(intern)}
                                                    className="intern-checkbox"
                                                />
                                                <label htmlFor={`intern-${index}`} className="checkbox-label">
                                                    {intern}
                                                </label>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                                <div className="selected-interns">
                                    <strong>Selected ({selectedInterns.length}): </strong>
                                    {selectedInterns.length > 0 ? selectedInterns.join(', ') : 'None'}
                                </div>
                            </div>
                            <div className="form-group">
                                 <label> Submission:</label>
                                <select>
                                    <option > git </option>
                                    <option>File upload</option>
                                </select>
                            </div>
                            <div className="form-group">
                                <label> Grading rubric:</label>
                                <select 
                                    value={selectedRubric} 
                                    onChange={(e) => setSelectedRubric(e.target.value)}
                                    className="rubric-dropdown"
                                >
                                    <option value="">Select a grading rubric</option>
                                    {rubrics.map(rubric => (
                                        <option key={rubric.id} value={rubric.id}>
                                            {rubric.name} ({rubric.totalMarks} marks, {rubric.criteria.length} criteria)
                                        </option>
                                    ))}
                                </select>
                                {selectedRubric && (
                                    <div className="selected-rubric-info">
                                        <strong>Selected Rubric: </strong>
                                        {rubrics.find(r => r.id.toString() === selectedRubric)?.name}
                                        <div className="rubric-preview">
                                            {rubrics.find(r => r.id.toString() === selectedRubric)?.criteria.map((criterion, index) => (
                                                <div key={criterion.id} className="preview-criterion-small">
                                                    <span>{criterion.name}</span>
                                                    <span>{criterion.marks} pts</span>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}
                                <div className="rubric-actions">
                                    
                                    
                                </div>
                            </div>
                            <div className="modal-actions">
                                <button className="cancel-btn" onClick={() => setIsCreateAssignmentOpen(false)}>
                                   Cancel
                                </button>
                                <button className="assign-btn">
                                    Assign
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
                                                value={criterion.name}
                                                onChange={(e) => updateCriterionName(criterion.id, e.target.value)}
                                                className="criterion-name-input"
                                            />
                                            <div className="marks-input-container">
                                                <label> Marks:</label>
                                                <input
                                                    type="number"
                                                    min="0"
                                                    max="100"
                                                    value={criterion.marks}
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
                                                    <span>Total Marks: {rubric.totalMarks}</span>
                                                    <span>Criteria: {rubric.criteria.length}</span>
                                                    <span>Created: {rubric.createdAt}</span>
                                                </div>
                                                <div className="criteria-preview">
                                                    {rubric.criteria.slice(0, 3).map((criterion, index) => (
                                                        <div key={criterion.id} className="preview-criterion">
                                                            <span className="preview-name">{criterion.name}</span>
                                                            <span className="preview-marks">{criterion.marks} pts</span>
                                                        </div>
                                                    ))}
                                                    {rubric.criteria.length > 3 && (
                                                        <div className="more-criteria">
                                                            +{rubric.criteria.length - 3} more criteria
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
            </div>
        </LocalizationProvider>
    );
}

export default SupervisorReports;