(function() {
    'use strict';

    function escapeHtml(str) {
        if (str == null || str === undefined) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

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
        vm.autoGeneratePassword = true;
        vm.passwordPreview = '';
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
                    { data: 'alternatePhone' },
                    { data: 'email' },
                    { data: 'roleName' },
                    { data: 'reporteeRoleName' },
                    {
                        data: 'lastGeneratedPassword',
                        orderable: false,
                        render: function(data, type, row) {
                            if (!data) {
                                return '<span class="text-muted">—</span>';
                            }
                            return (
                                '<span class="d-inline-flex align-items-start gap-1 flex-wrap">' +
                                '<code class="small text-break" style="max-width:220px;">' + escapeHtml(data) + '</code>' +
                                '<button type="button" class="btn btn-sm btn-light py-0 px-1" title="Copy password" ' +
                                'onclick="navigator.clipboard.writeText(this.parentElement.querySelector(\'code\').textContent);">' +
                                '<i class="mdi mdi-content-copy"></i></button></span>'
                            );
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        render: function(data, type, row) {
                            return `
                                <div class="btn-group">
                                    <button class="btn btn-sm btn-warning" title="Edit" ng-click="vm.editPerson(${row.id})">
                                        <i class="mdi mdi-pencil"></i>
                                    </button>
                                    <button class="btn btn-sm btn-info" title="Reset password" ng-click="vm.confirmResetPassword(${row.id})">
                                        <i class="mdi mdi-lock-reset"></i>
                                    </button>
                                    <button class="btn btn-sm btn-danger" title="Remove" ng-click="vm.removePerson(${row.id})">
                                        <i class="mdi mdi-close-thick"></i>
                                    </button>
                                    <button class="btn btn-sm btn-primary" title="Assign Program" ng-click="vm.assignProgram(${row.id})">
                                        <i class="mdi mdi-plus"></i> Assign Program
                                    </button>
                                    <a href="/Admin/AssignInstitution?id=${row.id}" class="btn btn-sm btn-success" title="Assign Institution">
                                        <i class="mdi mdi-building"></i> Assign Institution
                                    </a>
                                    <a href="/Admin/UserMenuPermissions/${row.id}" class="btn btn-sm btn-secondary" title="Menu Permission">
                                        <i class="mdi mdi-menu"></i> Menu Permission
                                    </a>
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

        vm.onAutoGeneratePasswordChange = function() {
            if (vm.autoGeneratePassword) {
                vm.userInfo.password = '';
                vm.userInfo.confirmPassword = '';
                vm.refreshPasswordPreview();
            } else {
                vm.passwordPreview = '';
            }
        };

        vm.refreshPasswordPreview = function() {
            PeopleService.getPasswordPreview()
                .then(function(d) {
                    vm.passwordPreview = d.password;
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Could not generate preview', 1);
                });
        };

        vm.fillManualPasswordFromPreview = function() {
            PeopleService.getPasswordPreview()
                .then(function(d) {
                    vm.userInfo.password = d.password;
                    vm.userInfo.confirmPassword = d.password;
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Could not generate password', 1);
                });
        };

        vm.confirmResetPassword = function(id) {
            Swal.fire({
                title: 'Reset password?',
                text: 'A new secure password will be generated and saved. The previous password will stop working.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, reset password'
            }).then(function(result) {
                if (!result.isConfirmed) return;
                PeopleService.resetUserPassword(id)
                    .then(function(response) {
                        if (response.success) {
                            vm.dataTable.ajax.reload();
                            var pwd = response.result && (response.result.generatedPassword || response.result.GeneratedPassword);
                            if (pwd) {
                                Swal.fire({
                                    title: 'New password',
                                    html: '<p class="mb-2">Copy and share this with the user securely.</p><code style="font-size:1.1rem;">' + escapeHtml(pwd) + '</code>',
                                    icon: 'success'
                                });
                            } else {
                                ShowNotification(response.message, 0);
                            }
                        } else {
                            ShowNotification(response.message, 1);
                        }
                    })
                    .catch(function(error) {
                        ShowNotification(error.message || 'Reset failed', 1);
                    });
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
                    vm.autoGeneratePassword = false;
                    vm.passwordPreview = '';
                    // Clear password fields for editing (user can leave blank to keep current password)
                    vm.userInfo.password = '';
                    vm.userInfo.confirmPassword = '';
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

            var userData = angular.copy(vm.userInfo);
            if (userData.id > 0 && (!vm.autoGeneratePassword) && (!userData.password || userData.password.trim() === '')) {
                delete userData.password;
                delete userData.confirmPassword;
            }

            vm.isSaving = true;
            PeopleService.saveUser(userData, vm.autoGeneratePassword)
                .then(function(response) {
                    vm.isSubmit = false;
                    vm.isSaving = false;
                    if (response.success) {
                        var pwd = response.result && (response.result.generatedPassword || response.result.GeneratedPassword);
                        if (pwd) {
                            Swal.fire({
                                title: userData.id > 0 ? 'User updated' : 'User created',
                                html: '<p class="mb-2">Password (copy and share securely):</p><code style="font-size:1.1rem;">' + escapeHtml(pwd) + '</code>',
                                icon: 'success'
                            });
                        } else {
                            ShowNotification(response.message, 0);
                        }
                        $('#addPeopleModal').modal('hide');
                        vm.dataTable.ajax.reload();
                        vm.userInfo.password = '';
                        vm.userInfo.confirmPassword = '';
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
                        password: '',
                        confirmPassword: '',
                        phone: '',
                        gender: null,
                        roleId: null,
                        reporteeRoleId: null,
                        isActive: true
                    };
                    vm.autoGeneratePassword = true;
                    vm.passwordPreview = '';
                    vm.refreshPasswordPreview();

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

        vm.normalizeDropdownList = function(items) {
            return (items || []).map(function(item) {
                var value = item.value != null ? item.value : item.Value;
                return {
                    value: parseInt(value, 10),
                    text: item.text || item.Text || ''
                };
            });
        };

        vm.ensureLocationInList = function(list, id) {
            id = parseInt(id, 10);
            if (!id) {
                return list || [];
            }

            list = list || [];
            if (!list.some(function(item) { return item.value === id; })) {
                list.push({
                    value: id,
                    text: 'Location ' + id
                });
            }

            return list;
        };

        vm.parseAssignedInstitutionIds = function(source) {
            if (!source) {
                return [];
            }

            var institutionIds = source.institutionIds || source.InstitutionIds;
            if (!institutionIds) {
                return [];
            }

            return String(institutionIds)
                .split(',')
                .map(function(id) { return String(id).trim(); })
                .filter(function(id) { return id.length > 0; });
        };

        vm.normalizeInstitutionTypes = function(items) {
            return (items || []).map(function(item) {
                var value = item.value != null ? item.value : item.Value;
                return {
                    value: parseInt(value, 10),
                    text: item.text || item.Text || ''
                };
            });
        };

        vm.normalizeInstitutionOptions = function(items) {
            return (items || []).map(function(item) {
                var value = item.value != null ? item.value : item.Value;
                return {
                    value: String(value),
                    text: item.text || item.Text || ''
                };
            });
        };

        vm.retainMatchingSelectedInstitutions = function(options) {
            var allowedValues = new Set((options || []).map(function(item) {
                return String(item.value);
            }));

            vm.institutionData.selectedInstitutions = (vm.institutionData.selectedInstitutions || []).filter(function(id) {
                return allowedValues.has(String(id));
            });
        };

        vm.mergeInstitutionOptions = function(primaryList, assignedList, selectedIds) {
            var merged = {};
            var lists = [primaryList || [], assignedList || []];

            lists.forEach(function(list) {
                vm.normalizeInstitutionOptions(list).forEach(function(item) {
                    merged[item.value] = item;
                });
            });

            (selectedIds || []).forEach(function(id) {
                var key = String(id);
                if (!merged[key]) {
                    merged[key] = {
                        value: key,
                        text: 'Institution ' + key
                    };
                }
            });

            return Object.keys(merged).map(function(key) {
                return merged[key];
            });
        };

        vm.initializeInstitutionSelect = function() {
            $timeout(function() {
                var institutionSelect = $('#institutionSelect');

                if (institutionSelect.data('select2')) {
                    institutionSelect.off('.institutionSelect');
                    institutionSelect.select2('destroy');
                }

                var select2Instance = institutionSelect.select2({
                    placeholder: 'Search and select institutions',
                    allowClear: true,
                    closeOnSelect: false,
                    width: '100%',
                    dropdownParent: $('#assignInstitutionModal')
                });

                if (vm.institutionData.selectedInstitutions &&
                    vm.institutionData.selectedInstitutions.length > 0) {
                    select2Instance.val(vm.institutionData.selectedInstitutions).trigger('change');
                }

                select2Instance.off('change.institutionSelect').on('change.institutionSelect', function(e) {
                    $timeout(function() {
                        var selectedValues = $(e.target).val();
                        vm.institutionData.selectedInstitutions = selectedValues ? selectedValues : [];
                    });
                });
            }, 300);
        };

        vm.resetLocationDropdowns = function() {
            vm.institutionData.stateId = null;
            vm.institutionData.districtId = null;
            vm.institutionData.blockId = null;
            vm.institutionData.villageId = null;
            vm.institutionData.selectedInstitutions = [];
            vm.states = [];
            vm.districts = [];
            vm.blocks = [];
            vm.villages = [];
            vm.institutions = [];
        };

        vm.loadDistrictsByDivision = function() {
            vm.institutionData.districtId = null;
            vm.institutionData.blockId = null;
            vm.institutionData.villageId = null;
            vm.institutionData.selectedInstitutions = [];
            vm.districts = [];
            vm.blocks = [];
            vm.villages = [];
            vm.institutions = [];

            if (!vm.institutionData.divisionId || !vm.institutionData.stateId) {
                return $q.resolve([]);
            }

            return PeopleService.getDistrictsByDivision(vm.institutionData.divisionId, vm.institutionData.stateId)
                .then(function(data) {
                    vm.districts = vm.normalizeDropdownList(data);
                    return vm.districts;
                });
        };

        vm.onDivisionChange = function() {
            vm.resetLocationDropdowns();
            if (!vm.institutionData.divisionId) {
                return;
            }

            PeopleService.getStatesByDivision(vm.institutionData.divisionId)
                .then(function(states) {
                    vm.states = vm.normalizeDropdownList(states);
                    if (vm.states.length === 0) {
                        ShowNotification('No location assigned to this division.', 1);
                        return;
                    }
                    if (vm.states.length === 1) {
                        vm.institutionData.stateId = vm.states[0].value;
                        vm.loadDistrictsByDivision();
                    }
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Error fetching division states', 1);
                });
        };

        vm.assignInstitution = function(id, fullName, roleName) {
            vm.selectedUserId = id;
            vm.selectedPerson = {
                fullName: fullName,
                roleName: roleName
            };

            vm.institutionData = {
                divisionId: null,
                stateId: null,
                districtId: null,
                blockId: null,
                villageId: null,
                institutionTypeId: null,
                selectedInstitutions: []
            };
            vm.divisions = [];
            vm.states = [];
            vm.districts = [];
            vm.blocks = [];
            vm.villages = [];
            vm.institutions = [];
            var assignedInstitutions = [];

            PeopleService.getLocationData(id)
                .then(function(response) {
                    vm.divisions = vm.normalizeDropdownList(response.divisions || []);
                    vm.institutionTypes = vm.normalizeInstitutionTypes(response.institutionTypes || []);
                    assignedInstitutions = response.assignedInstitutions || response.AssignedInstitutions || [];

                    if (response.peopleInstitution && response.peopleInstitution.divisionId) {
                        var pi = response.peopleInstitution;
                        vm.institutionData.divisionId = parseInt(pi.divisionId, 10);
                        vm.institutionData.selectedInstitutions = vm.parseAssignedInstitutionIds(pi);

                        return PeopleService.getStatesByDivision(pi.divisionId)
                            .then(function(states) {
                                vm.states = vm.ensureLocationInList(
                                    vm.normalizeDropdownList(states),
                                    pi.stateId);
                                vm.institutionData.stateId = parseInt(pi.stateId, 10);
                                return PeopleService.getDistrictsByDivision(pi.divisionId, pi.stateId);
                            })
                            .then(function(districts) {
                                vm.districts = vm.ensureLocationInList(
                                    vm.normalizeDropdownList(districts),
                                    pi.districtId);
                                vm.institutionData.districtId = parseInt(pi.districtId, 10);
                                return PeopleService.getBlocksByDivision(pi.divisionId, pi.districtId);
                            })
                            .then(function(blocks) {
                                vm.blocks = vm.ensureLocationInList(
                                    vm.normalizeDropdownList(blocks),
                                    pi.blockId);
                                vm.institutionData.blockId = parseInt(pi.blockId, 10);
                                return PeopleService.getVillagesByDivision(pi.divisionId, pi.blockId);
                            })
                            .then(function(villages) {
                                vm.villages = vm.ensureLocationInList(
                                    vm.normalizeDropdownList(villages),
                                    pi.villageId);
                                vm.institutionData.villageId = parseInt(pi.villageId, 10);
                                vm.institutionData.institutionTypeId = parseInt(pi.institutionTypeId, 10) || null;

                                if (pi.villageId && pi.institutionTypeId) {
                                    return PeopleService.getInstitutionsByVillageId(pi.villageId, pi.institutionTypeId);
                                }

                                return [];
                            })
                            .then(function(institutions) {
                                vm.institutions = vm.mergeInstitutionOptions(
                                    institutions,
                                    assignedInstitutions,
                                    vm.institutionData.selectedInstitutions
                                );
                            });
                    }
                })
                .then(function() {
                    $timeout(function() {
                        var modal = new bootstrap.Modal(document.getElementById('assignInstitutionModal'));
                        modal.show();
                        vm.initializeInstitutionSelect();
                    }, 0);
                })
                .catch(function(error) {
                    ShowNotification(error.message || 'Error loading location data', 1);
                });
        };

        // Add cascade dropdown handlers
       

        // Add similar handlers for other dropdowns
        vm.onStateChange = function() {
            vm.loadDistrictsByDivision();
        };

        vm.onDistrictChange = function() {
            vm.institutionData.blockId = null;
            vm.institutionData.villageId = null;
            vm.institutionData.selectedInstitutions = [];
            vm.blocks = [];
            vm.villages = [];
            vm.institutions = [];

            if (vm.institutionData.divisionId && vm.institutionData.districtId) {
                PeopleService.getBlocksByDivision(vm.institutionData.divisionId, vm.institutionData.districtId)
                    .then(function(data) {
                        $timeout(function() {
                            vm.blocks = vm.normalizeDropdownList(data);
                        });
                    });
            }
        };

        vm.onBlockChange = function() {
            vm.institutionData.villageId = null;
            vm.institutionData.selectedInstitutions = [];
            vm.villages = [];
            vm.institutions = [];

            if (vm.institutionData.divisionId && vm.institutionData.blockId) {
                PeopleService.getVillagesByDivision(vm.institutionData.divisionId, vm.institutionData.blockId)
                    .then(function(data) {
                        $timeout(function() {
                            vm.villages = vm.normalizeDropdownList(data);
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
                            vm.institutions = vm.normalizeInstitutionOptions(data);
                            vm.retainMatchingSelectedInstitutions(vm.institutions);
                            vm.initializeInstitutionSelect();
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