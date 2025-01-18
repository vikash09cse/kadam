(function() {
    angular.module('app', []).factory('loaderInterceptor', ['$q', '$rootScope', function($q, $rootScope) {
        return {
            request: function(config) {
                $rootScope.isLoading = true;
                return config;
            },
            response: function(response) {
                $rootScope.isLoading = false;
                return response;
            },
            responseError: function(rejection) {
                $rootScope.isLoading = false;
                return $q.reject(rejection);
            }
        };
    }]);
})();