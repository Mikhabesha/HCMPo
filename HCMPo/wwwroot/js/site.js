// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Cascading dropdown functionality for OrganizationUnit and JobTitle
$(document).ready(function() {
    // Function to update job titles based on selected organization unit
    function updateJobTitles(orgUnitId) {
        var jobTitleSelect = $('#JobTitleId');
        
        // Clear current options
        jobTitleSelect.empty();
        jobTitleSelect.append('<option value="">-- Select Job Title --</option>');
        
        if (!orgUnitId) {
            return;
        }
        
        // Show loading indicator
        jobTitleSelect.prop('disabled', true);
        
        // Fetch job titles for the selected organization unit
        $.ajax({
            url: '/Employees/GetJobTitlesByOrgUnit',
            type: 'GET',
            data: { orgUnitId: orgUnitId },
            success: function(data) {
                // Add new options
                $.each(data, function(index, item) {
                    jobTitleSelect.append($('<option></option>')
                        .attr('value', item.value)
                        .text(item.text));
                });
                
                // Re-enable select
                jobTitleSelect.prop('disabled', false);
            },
            error: function() {
                console.error('Failed to load job titles');
                jobTitleSelect.prop('disabled', false);
            }
        });
    }
    
    // Attach event handler to organization unit dropdown
    $('#OrganizationUnitId').on('change', function() {
        var selectedOrgUnitId = $(this).val();
        updateJobTitles(selectedOrgUnitId);
    });
    
    // Initialize job titles if organization unit is pre-selected (for edit forms)
    var initialOrgUnitId = $('#OrganizationUnitId').val();
    if (initialOrgUnitId) {
        updateJobTitles(initialOrgUnitId);
    }
});
