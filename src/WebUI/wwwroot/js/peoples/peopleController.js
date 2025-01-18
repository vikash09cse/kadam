(function() {
    'use strict';

    angular
        .module('peopleApp')
        .controller('peopleController', PeopleController);

    PeopleController.$inject = ['PeopleService', '$scope', '$q', '$compile', '$timeout'];

    function PeopleController(PeopleService, $scope, $q, $compile, $timeout) {
        var vm = this;

        // Initialize variables
        vm.genders = [];
        vm.roles = [];
        vm.reportRoles = [];
        vm.userInfo = {};
        vm.isSaving = false;
        vm.isSubmit = false;
        vm.programs = {
            neTT: false,
            kadam: false
        };

        // Initialize DataTable
        vm.initDataTable = function() {
            vm.dataTable = $('#peopleTable').DataTable({
                serverSide: true,
                processing: true,
                paging: true,
                ajax: function(data, callback, settings) {
                    var params = {
                        draw: data.draw,
                        start: data.start,
                        length: data.length,
                        searchValue: data.search.value
                    };
                    PeopleService.getUserList(params)
                        .then(function(response) {
                            callback(response);
                        })
                        .catch(function(error) {
                            console.error('Error fetching data:', error);
                        });
                },
                columns: [
                    { data: 'rowNumber' },
                    { data: 'fullName' },
                    { data: 'userName' },
                    { data: 'genderName' },
                    { data: 'phone' },
                    { data: 'email' },
                    { data: 'roleName' },
                    { data: 'reporteeRoleName' },
                    {
                        data: null,
                        orderable: false,
                        render: function(data, type, row) {
                            return `
                                <div class="btn-group">
                                    <button class="btn btn-sm btn-warning" title="Edit" ng-click="vm.editPerson(${row.id})">
                                        <i class="mdi mdi-pencil"></i>
                                    </button>
                                    <button class="btn btn-sm btn-danger" title="Remove" ng-click="vm.removePerson(${row.id})">
                                        <i class="mdi mdi-close-thick"></i>
                                    </button>
                                    <button class="btn btn-sm btn-primary" title="Assign Program" ng-click="vm.assignProgram(${row.id})">
                                        <i class="mdi mdi-plus"></i> Assign Program
                                    </button>
                                    <button class="btn btn-sm btn-success" title="Assign Institution" 
                                            ng-click="vm.assignInstitution('${row.id}', '${row.fullName}', '${row.roleName}')">
                                        <i class="mdi mdi-building"></i> Assign Institution
                                    </button>
                                </div>
                            `;
                        }
                    }
                ],
                lengthMenu: [
                    [10, 25, 50, 100],
                    [10, 25, 50, 100]
                ],
                language: {
                    searchPlaceholder: "Search records",
                    info: "Showing _START_ to _END_ of _TOTAL_ records",
                    lengthMenu: "Show _MENU_ records",
                    infoEmpty: "Showing 0 to 0 of 0 records",
                    emptyTable: "No records found!"
                },
                bStateSave: true,
                fnStateSave: function (oSettings, oData) {
                    localStorage.setItem('PeopleTable_' + window.location.pathname, JSON.stringify(oData));
                },
                fnStateLoad: function (oSettings) {
                    return JSON.parse(localStorage.getItem('PeopleTable_' + window.location.pathname));
                },
                createdRow: function(row, data, dataIndex) {
                    $compile(angular.element(row).contents())($scope);
                }
            });
        };

        // Edit Person
        vm.editPerson = function(id) {
            PeopleService.getInitialData(id)
                .then(function(response) {
                    vm.genders = response.genders;
                    vm.roles = response.roles;
                    vm.reportRoles = response.reportRoles;
                    vm.userInfo = response.userInfo;
                    $('#addPeopleModal').modal('show');
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Error loading user data', 1);
                });
        };

        // Remove Person
        vm.removePerson = function(id) {
            Swal.fire({
                title: 'Are you sure?',
                text: "You won't be able to revert this!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    PeopleService.deleteUser(id)
                        .then(function(response) {
                            if (response.success) {
                                vm.dataTable.ajax.reload();
                                ShowNotification(response.message, 0);
                            } else {
                                ShowNotification(response.message, 1);
                            }
                        })
                        .catch(function(error) {
                            ShowNotification(error.message, 1);
                        });
                }
            });
        };

        // Submit Form
        vm.submitForm = function(form) {
            vm.isSubmit = true;
            if (form.$invalid) {
                var firstError = angular.element("[name='" + form.$name + "']").find('.ng-invalid:visible:first');
                if (firstError.length) {
                    firstError.focus();
                }
                return;
            }

            vm.isSaving = true;
            PeopleService.saveUser(vm.userInfo)
                .then(function(response) {
                    vm.isSubmit = false;
                    vm.isSaving = false;
                    if (response.success) {
                        ShowNotification(response.message, 0);
                        $('#addPeopleModal').modal('hide');
                        vm.dataTable.ajax.reload();
                    } else {
                        ShowNotification(response.message, 1);
                    }
                })
                .catch(function(error) {
                    vm.isSaving = false;
                    vm.isSubmit = false;
                    ShowNotification(error.message || 'An error occurred while saving', 1);
                });
        };

        // Initialize Controller
        vm.init = function() {
            vm.initDataTable();
        };

        vm.init();

        // Add this function inside the peopleController
        vm.openAddPeopleModal = function() {
            // Reset form state
            vm.isSubmit = false;
            vm.isSaving = false;

            // Get initial data with no ID (for new user)
            PeopleService.getInitialData()
                .then(function(response) {
                    // Set dropdown options
                    vm.genders = response.genders;
                    vm.roles = response.roles;
                    vm.reportRoles = response.reportRoles;

                    // Reset user info
                    vm.userInfo = {
                        id: 0,
                        firstName: '',
                        lastName: '',
                        userName: '',
                        email: '',
                        phone: '',
                        gender: null,
                        roleId: null,
                        reporteeRoleId: null,
                        isActive: true
                    };

                    // Initialize modal with options
                    var modal = new bootstrap.Modal(document.getElementById('addPeopleModal'), {
                        backdrop: 'static',
                        keyboard: false
                    });
                    
                    // Show the modal
                    modal.show();
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Error loading initial data', 1);
                });
        };

        vm.assignProgram = function(id) {
            vm.selectedUserId = id;
            
            PeopleService.getUserPrograms(id)
                .then(function(response) {
                    vm.programs = response;
                    // Show the modal
                    var modal = new bootstrap.Modal(document.getElementById('assignProgramModal'));
                    modal.show();
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Error loading programs', 1);
                });
            
            
        };

        vm.submitAssignProgram = function() {
            console.log(vm.programs);
            const selectedPrograms = vm.programs.filter(program => program.isSelected).map(program => ({
                UserId: vm.selectedUserId,
                ProgramId: program.programId
            }));
            if (selectedPrograms.length > 0) {
                PeopleService.saveUserPrograms(selectedPrograms)
                    .then(function(response) {
                        // Close the modal
                        $('#assignProgramModal').modal('hide');
                        
                        // Show success notification
                        ShowNotification('Programs assigned successfully', 0);
                    })
                    .catch(function(error) {
                        ShowNotification(error.message || 'Error assigning programs', 1);
                    });
            } else {
                ShowNotification('No programs selected to assign', 1);
            }
        };

        vm.institutionData = {};
        vm.divisions = [];
        vm.states = [];
        vm.districts = [];
        vm.blocks = [];
        vm.villages = [];
        vm.institutionTypes = [];
        vm.institutions = [];

        vm.assignInstitution = function(id, fullName, roleName) {
            vm.selectedUserId = id;
            vm.selectedPerson = {
                fullName: fullName,
                roleName: roleName
            };
            
            // Reset form
            vm.institutionData = {
                divisionId: '',
                stateId: '',
                districtId: '',
                blockId: '',
                villageId: '',
                institutionTypeId: '',
                selectedInstitutions: []
            };
            vm.divisions = [];
            vm.states = [];
            vm.districts = [];
            vm.blocks = [];
            vm.villages = [];
            vm.institutions = [];
            
            // Get location data
            PeopleService.getLocationData(id)
                .then(function (response) {
                    // Set initial dropdown options
                    vm.divisions = response.divisions || [];
                    vm.states = response.states || [];
                    vm.institutionTypes = response.institutionTypes || [];
                    
                    if (response.peopleInstitution) {
                        // Set initial values first
                        vm.institutionData = {
                            divisionId: response.peopleInstitution.divisionId ? response.peopleInstitution.divisionId + '' : '',
                            stateId: response.peopleInstitution.stateId ? response.peopleInstitution.stateId + '' : '',
                            
                        };
                        console.log(vm.institutionData);


                        // Chain the dropdown population
                        return PeopleService.getDistrictsByState(response.peopleInstitution.stateId)
                            .then(function(districts) {
                                vm.districts = districts || [];
                                vm.institutionData.districtId = response.peopleInstitution.districtId ? response.peopleInstitution.districtId + '' : '';
                                return PeopleService.getBlocksByDistrict(response.peopleInstitution.districtId);
                            })
                            .then(function(blocks) {    
                                vm.blocks = blocks || [];
                                vm.institutionData.blockId = response.peopleInstitution.blockId ? response.peopleInstitution.blockId + '' : '';
                                return PeopleService.getVillagesByBlock(response.peopleInstitution.blockId);
                            })
                            .then(function(villages) {
                                vm.villages = villages || [];
                                vm.institutionData.villageId = response.peopleInstitution.villageId ? response.peopleInstitution.villageId + '' : '';
                                vm.institutionData.institutionTypeId = response.peopleInstitution.institutionTypeId ? response.peopleInstitution.institutionTypeId + '' : '';
                                if (response.peopleInstitution.villageId && response.peopleInstitution.institutionTypeId) {
                                    return PeopleService.getInstitutionsByVillageId(
                                        response.peopleInstitution.villageId,
                                        response.peopleInstitution.institutionTypeId
                                    );
                                }
                            })
                            .then(function(institutions) {
                                if (institutions) { 
                                    vm.institutions = institutions;
                                    vm.institutionData.selectedInstitutions = response.peopleInstitution.institutionIds ? 
                                        response.peopleInstitution.institutionIds.split(',') : [];
                                }
                            });
                    }
                })
                .then(function() {
                    // Show modal after all data is loaded
                    var modal = new bootstrap.Modal(document.getElementById('assignInstitutionModal'));
                    modal.show();
                    
                    // Initialize Select2 after modal is shown
                    $('#assignInstitutionModal').on('shown.bs.modal', function() {
                        $timeout(function() {
                            // Destroy existing Select2 instance if it exists
                            if ($('#institutionSelect').data('select2')) {
                                $('#institutionSelect').select2('destroy');
                            }

                            const select2Instance = $('#institutionSelect').select2({
                                placeholder: 'Select institutions',
                                allowClear: true,
                                width: '100%',
                                dropdownParent: $('#assignInstitutionModal')
                            });

                            // Set the initial values after Select2 initialization
                            if (vm.institutionData.selectedInstitutions && 
                                vm.institutionData.selectedInstitutions.length > 0) {
                                select2Instance.val(vm.institutionData.selectedInstitutions).trigger('change');
                            }

                            // Handle Select2 changes
                            select2Instance.on('change', function(e) {
                                $timeout(function() {
                                    const selectedValues = $(e.target).val(); // Use e.target to get the correct context
                                    console.log(selectedValues);
                                    // Update the AngularJS model
                                    vm.institutionData.selectedInstitutions = selectedValues ? selectedValues : [];
                                });
                            });
                        }, 100);
                    });
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Error loading location data', 1);
                });
        };

        // Add cascade dropdown handlers
       

        // Add similar handlers for other dropdowns
        vm.onStateChange = function() {
            vm.institutionData.districtId = '';
            vm.institutionData.blockId = '';
            vm.institutionData.villageId = '';
            vm.districts = [];
            vm.blocks = [];
            vm.villages = [];
            vm.institutions = [];
            
            if (vm.institutionData.stateId) {
                PeopleService.getDistrictsByState(vm.institutionData.stateId)
                    .then(function(data) {
                        $timeout(function() {
                            vm.districts = data || [];

                        });
                    });
            }
        };

        vm.onDistrictChange = function() {
            vm.institutionData.blockId = '';
            vm.institutionData.villageId = '';
            vm.blocks = [];
            vm.villages = [];
            vm.institutions = [];
            
            if (vm.institutionData.districtId) {
                PeopleService.getBlocksByDistrict(vm.institutionData.districtId)
                    .then(function(data) {
                        $timeout(function() {
                            vm.blocks = data || [];
                        });
                    });
            }
        };

        vm.onBlockChange = function() {
            vm.institutionData.villageId = '';
            vm.villages = [];
            vm.institutions = [];
            
            if (vm.institutionData.blockId) {
                PeopleService.getVillagesByBlock(vm.institutionData.blockId)
                    .then(function(data) {
                        $timeout(function() {
                            vm.villages = data || [];
                        });
                    });
            }
        };

        vm.onVillageChange = function() {
            vm.institutions = [];
            if (vm.institutionData.villageId && vm.institutionData.institutionTypeId) {
                vm.onInstitutionTypeChange();
            }
        };

        vm.onInstitutionTypeChange = function() {
            vm.institutions = [];
            if (vm.institutionData.villageId && vm.institutionData.institutionTypeId) {
                PeopleService.getInstitutionsByVillageId(vm.institutionData.villageId, vm.institutionData.institutionTypeId)
                    .then(function(data) {
                        $timeout(function() {
                            vm.institutions = data || [];
                        });
                    });
            }
        };

        vm.submitAssignInstitution = function () {
            console.log(vm.institutionData.selectedInstitutions);
            if (!vm.institutionForm.$valid) {
                ShowNotification('Please fill all required fields', 1);
                return;
            }

            if (!vm.institutionData.selectedInstitutions || vm.institutionData.selectedInstitutions.length === 0) {
                ShowNotification('Please select at least one institution', 1);
                return;
            }

            var data = {
                userId: +vm.selectedUserId,
                divisionId: +vm.institutionData.divisionId,
                stateId: +vm.institutionData.stateId,
                districtId: +vm.institutionData.districtId,
                blockId: +vm.institutionData.blockId,
                villageId: +vm.institutionData.villageId,
                institutionTypeId: +vm.institutionData.institutionTypeId,
                institutionIds: vm.institutionData.selectedInstitutions.join(',')
            };
            console.log(data);
            PeopleService.savePeopleInstitution(data)
                .then(function(response) {
                    if (response.success) {
                        ShowNotification('Institutions assigned successfully', 0);
                        $('#assignInstitutionModal').modal('hide');
                        vm.dataTable.ajax.reload();
                    } else {
                        ShowNotification(response.message, 1);
                    }
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Error assigning institutions', 1);
                });
        };
    }
})();