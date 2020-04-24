var AutoCSer;
(function (AutoCSer) {
    var OpenApi = (function () {
        function OpenApi(StateAjaxCallName) {
            if (StateAjaxCallName === void 0) { StateAjaxCallName = 'pub.GetOpenApiState'; }
            this.StateAjaxCallName = StateAjaxCallName;
            this.APIs = {};
        }
        OpenApi.prototype.Login = function (Name) {
            var Api = this.APIs[Name];
            if (Api && Api.Url) {
                if (Api.IsState == null || Api.IsState)
                    HttpRequest.Post(this.StateAjaxCallName, null, Pub.ThisFunction(this, this.OnState, [Api]));
                else
                    location = Api.Url;
            }
            else
                Pub.Alert('未找到第三方 ' + Name + ' API信息');
        };
        OpenApi.prototype.OnState = function (Value, Api) {
            if (Value = Value.__AJAXRETURN__) {
                if (this.IsLoginCookie == null || this.IsLoginCookie)
                    Cookie.Default.Write({ Name: 'OpenLoginUrl', Value: location.toString() });
                location = Api.Url + '&state=' + Value;
            }
            else
                HttpRequest.CheckError(Value);
        };
        OpenApi.prototype.Location = function (Url) {
            var Value = Cookie.Default.Read('OpenLoginUrl');
            if (Value)
                Cookie.Default.Write({ Name: 'OpenLoginUrl' });
            location.replace(Value || Url);
        };
        OpenApi.Default = new OpenApi();
        return OpenApi;
    }());
    OpenApi = OpenApi;
})(AutoCSer || (AutoCSer = {}));
