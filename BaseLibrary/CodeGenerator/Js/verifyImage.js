//验证图片	<img verifyimage="{Width:80,Height:20,ButtonId:'YYY'}" id="XXX" />
var AutoCSer;
(function (AutoCSer) {
    var VerifyImage = (function () {
        function VerifyImage(Parameter) {
            Pub.GetParameter(this, VerifyImage.DefaultParameter, Parameter);
            Pub.GetEvents(this, VerifyImage.DefaultEvents, Parameter);
            this.Start(this.Event || DeclareEvent.Default);
        }
        VerifyImage.prototype.Start = function (Event) {
            if (!Event.IsGetOnly) {
                var Element = HtmlElement.$Id(this.Id), Image = Element.Element0();
                if (Image != this.Element) {
                    this.Element = Image;
                    Element.Set('alt', '验证码').Set('border', 0);
                    if (this.Width)
                        Element.Set('width', this.Width);
                    if (this.Height)
                        Element.Set('height', this.Height);
                    HtmlElement.$Id(this.ButtonId).Cursor('pointer').AddEvent('click', Pub.ThisFunction(this, this.ClickButton));
                    this.Show(false);
                }
            }
        };
        VerifyImage.prototype.ClickButton = function () {
            this.Show(true);
            this.OnClick.Function();
        };
        VerifyImage.prototype.Show = function (IsRefresh) {
            var Verify = IsRefresh ? null : Cookie.Default.Read('VerifyImage');
            if (!Verify)
                Verify = (new Date).getTime();
            Cookie.Default.Write({ Name: 'VerifyImage', Value: Verify, Expires: (new Date).AddMinutes(20) });
            HtmlElement.$Id(this.Id).Set('src', '/verifyImage?t=' + Verify).Display(1);
        };
        VerifyImage.prototype.Clear = function () {
            Cookie.Default.Write({ Name: 'VerifyImage' });
        };
        VerifyImage.DefaultParameter = { Id: null, Event: null, Width: null, Height: null, ButtonId: null };
        VerifyImage.DefaultEvents = { OnClick: null };
        return VerifyImage;
    }());
    VerifyImage = VerifyImage;
    new Declare(VerifyImage, 'VerifyImage', 'mouseover', 'AttributeName');
})(AutoCSer || (AutoCSer = {}));
