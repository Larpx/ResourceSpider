/// <reference path = "./base.page.ts" />
'use strict';
//默认密码输入框	<input defaultpassword="{PasswordId:'YYY'}" id="XXX" type="text" /><input id="YYY" type="text" />
var AutoCSer;
(function (AutoCSer) {
    var DefaultPassword = (function () {
        function DefaultPassword(Parameter) {
            Pub.GetParameter(this, DefaultPassword.DefaultParameter, Parameter);
            this.Start(this.Event || DeclareEvent.Default);
        }
        DefaultPassword.prototype.Start = function (Event) {
            if (!Event.IsGetOnly) {
                var Element = HtmlElement.$Id(this.Id), Input = Element.Element0(), Password = HtmlElement.$Id(this.PasswordId);
                if (Input != this.Element) {
                    this.Element = Input;
                    Password.AddEvent('blur', Pub.ThisFunction(this, this.OnBlur));
                }
                Element.Display(0);
                Password.Display(1).Focus0();
            }
        };
        DefaultPassword.prototype.OnBlur = function () {
            var Password = HtmlElement.$Id(this.PasswordId);
            if (!Password.Element0().value) {
                Password.Display(0);
                HtmlElement.$Id(this.Id).Display(1);
            }
        };
        DefaultPassword.DefaultParameter = { Id: null, Event: null, PasswordId: null };
        return DefaultPassword;
    }());
    DefaultPassword = DefaultPassword;
    new Declare(DefaultPassword, 'DefaultPassword', 'focus', 'Src');
})(AutoCSer || (AutoCSer = {}));
