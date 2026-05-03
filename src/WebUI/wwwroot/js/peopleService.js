(function() {
    'use strict';

    angular
        .module('peopleApp')
        .factory('PeopleService', PeopleService);

    PeopleService.$inject = ['$http', '$q'];

    function PeopleService($http, $q) {
        var service = {
            getInitialData: getInitialData,
            getUserList: getUserList,
            saveUser: saveUser,
            getPasswordPreview: getPasswordPreview,
            resetUserPassword: resetUserPassword,
            deleteUser: deleteUser,
            getUserPrograms: getUserPrograms,
            saveUserPrograms: saveUserPrograms,
            getDistrictsByState: getDistrictsByState,
            getBlocksByDistrict: getBlocksByDistrict,
            getVillagesByBlock: getVillagesByBlock,
            getLocationData: getLocationData,
            getInstitutionsByVillageId: getInstitutionsByVillageId,
            savePeopleInstitution: savePeopleInstitution
        };

        return service;

        function getInitialData(id) {
            return $http.get('?handler=InitialData', { params: { id: id } })
                .then(response => response.data)
                .catch(error => $q.reject(error));
        }

        function getUserList(params) {
            return $http.get('?handler=UserList', { params: params })
                .then(response => response.data)
                .catch(error => $q.reject(error));
        }

        function saveUser(userInfo, autoGeneratePassword) {
            var payload = angular.copy(userInfo);
            var pwd = payload.password;
            delete payload.password;
            delete payload.confirmPassword;

            var requestData = {
                user: payload,
                password: autoGeneratePassword ? null : (pwd || null),
                autoGeneratePassword: !!autoGeneratePassword
            };

            return $http.post('?handler=SaveUser', requestData, {
                headers: { 'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val() }
            })
            .then(response => response.data)
            .catch(error => $q.reject(error));
        }

        function getPasswordPreview() {
            return $http.get('?handler=GeneratePasswordPreview')
                .then(response => response.data)
                .catch(error => $q.reject(error));
        }

        function resetUserPassword(id) {
            return $http.post('?handler=ResetUserPassword&id=' + id, null, {
                headers: { 'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val() }
            })
            .then(response => response.data)
            .catch(error => $q.reject(error));
        }

        function deleteUser(id) {
            return $http.post('?handler=DeleteUser&id=' + id, null, {
                headers: { 'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val() }
            })
            .then(response => response.data)
            .catch(error => $q.reject(error));
        }

        function getUserPrograms(userId) {
            return $http.get('?handler=UserPrograms', { params: { userId: userId } })
                .then(response => response.data)
                .catch(error => $q.reject(error));
        }

        function saveUserPrograms(userPrograms) {
            return $http.post('?handler=SaveUserPrograms', userPrograms, {
                headers: { 'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val() }
            })
            .then(response => response.data)
            .catch(error => $q.reject(error));
        }

        function getDistrictsByState(stateId) {
            return $http.get('?handler=DistrictListByState&stateId=' + stateId)
                .then(response => response.data)
                .catch(error => $q.reject(error));
        }

        function getBlocksByDistrict(districtId) {
            return $http.get('?handler=BlockListByDistrict&districtId=' + districtId)
                .then(response => response.data)
                .catch(error => $q.reject(error));
        }

        function getVillagesByBlock(blockId) {
            return $http.get('?handler=VillagesByBlock&blockId=' + blockId)
                .then(response => response.data)
                .catch(error => $q.reject(error));
        }

        function getLocationData(userId) {
            return $http.get('?handler=LocationData', { params: { userId: userId } })
                .then(response => response.data)
                .catch(error => $q.reject(error));
        }
        function getInstitutionsByVillageId(villageId, institutionTypeId) {
            return $http.get('?handler=InstitutionsByVillageId', { 
                params: { 
                    villageId: villageId, 
                    institutionTypeId: institutionTypeId 
                }
            })
            .then(response => response.data)
            .catch(error => $q.reject(error));
        }

        function savePeopleInstitution(peopleInstitution) {
            return $http.post('?handler=SavePeopleInstitution', peopleInstitution, {
                headers: { 'RequestVerificationToken': $('input:hidden[name="__RequestVerificationToken"]').val() }
            })
            .then(response => response.data)
            .catch(error => $q.reject(error));
        }
        
    }
})();