using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFApp09
{
    public class ModelLoginning : IModelRecord
    {
        private string _userLogin;
        private string _userPassword;
        private ulong _loginToken;


        public ModelLoginning()
        {
            this._userLogin = "";
            this._userPassword = "";
            this._loginToken = 0;
        }

        public string UserLogin{
            get {
                return _userLogin;
            }
            set {
                this._userLogin = value;
                generateToken(this._userLogin, this._userPassword, out this._loginToken);
            }
        }
        public string UserPassword
        {
            get {
                return _userPassword;
            }
            set
            {
                this._userPassword = value;
                generateToken(this._userLogin, this._userPassword, out this._loginToken);
            }
        }
        public ulong LoginToken
        {
            get
            {
                if( this._loginToken != 0)
                {
                    return this._loginToken;
                }
                ulong ulToken;
                bool res = generateToken(this._userLogin, this._userPassword, out ulToken);
                if(res)
                {
                    this._loginToken = ulToken;
                }
                return this._loginToken;
            }
        }
        private bool generateToken(in string sLogin, in string sPassword, out ulong ulToken)
        {
            ulToken = 0UL;
            bool res = false;
            if ((sLogin != null || sLogin.Length > 0) && (sPassword != null || sPassword.Length > 0))
            {
                ulToken = (ulong)(Math.Abs(this._userLogin.GetHashCode()) << 31);
                ulToken += (ulong)Math.Abs(this._userPassword.GetHashCode());
                res = true;
            }
            return res;
        }
    }
}
