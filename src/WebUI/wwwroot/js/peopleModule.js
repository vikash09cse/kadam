(function() {
    'use strict';

    // Define the main module with its dependencies
    angular.module('peopleApp', ['app'])
        .config(['$httpProvider', function($httpProvider) {
            // Configure HTTP Interceptors
            $httpProvider.interceptors.push('loaderInterceptor');
        }])
        .run(['$rootScope', function($rootScope) {
            // Initialize global variables
            $rootScope.isLoading = false;
        }]);
})();