
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private const string separator = ",";
        private const int cSectorWarmUpFlag = 107;
        private const string valueOfOne = "1";
        private const string valueZero = "0";
        private const int andValueOneA = 90;
        private const int andValueTwoA = 91;
        private const int feedFordwardA = 65;
        private const int andValueOneB = 81;
        private const int andTwoValueB = 82;
        private const int feedForwardB = 68;
        private const int andValueOneC = 108;
        private const int andValueTwoC = 109;
        private const int feedBackA = 68;
        private const int feedBackB = 77;
        private const int feedBackC = 88;
        private const int feedForwardC = 65;
        private readonly int aValue = 92;
        private readonly int bValue = 84;
        private readonly int cValue = 111;
        private int byteLength = 8;
        private Dictionary<int, string> special =
            new Dictionary<int, string>()
            {
                { 237,"í" },
                { 238,"í" }
            };
        
        private string[] arrFirstSector = new string[92];
        string[] arrSecondSector = new string[84];
        string[] arrThirdSector = new string[111];
        
        string[] arrFirstSectorShadow = new string[92];
        string[] arrSecondSectorShadow = new string[84];
        string[] arrThirdSectorShadow = new string[111];

        public ActionResult Index()
        {
            string prueba = "á";

            byte[] a = System.Text.Encoding.UTF8.GetBytes(prueba);

            string prueba2 = System.Text.Encoding.UTF8.GetString(a);

            return View();
        }

        public ActionResult Encript([Bind] Trivium trivium)
        {
            //Defualt values
            if(trivium.Key == "")
            {
                trivium.Key = "GREJCALDUR";
            }
            else if(trivium.VI == "")
            {
                trivium.VI = "DURCALGREJ";
            }

            var result = Execute(trivium.Key,trivium.VI, trivium.Clear);
            return View("Index", result);
        }

        public ActionResult Decript([Bind] Trivium trivium)
        {

            //Defualt values
            if (trivium.Key == "")
            {
                trivium.Key = "GREJCALDUR";
            }
            else if (trivium.VI == "")
            {
                trivium.VI = "DURCALGREJ";
            }


            var result = Execute(trivium.Key, trivium.VI, trivium.Encrypted, true);
            return View("Index", result);
        }

        public static string Base64Encode(byte[] plainText)
        {
            return Convert.ToBase64String(plainText);
        }

        public static byte[] Base64Decode(string base64EncodedData)
        {
            return Convert.FromBase64String(base64EncodedData);
        }

        private string RunTrivium(bool isWarmUp,string VI="",string key="")
        {
           


            var exit = "";
            var firstSectorExit = 0;
            var secondSectorExit = 0;
            var thirdSectorExit = 0;

            //entries of each sector
            var firsSectorEntry = 0;
            var secondSectorEntry = 0;
            var thirdSectorEntry = 0;

           

            firstSectorExit = AND(arrFirstSector[andValueOneA], arrFirstSector[andValueTwoA]);
            firstSectorExit = XOR(arrFirstSector[feedFordwardA], arrFirstSector[arrFirstSector.Length - 1], firstSectorExit.ToString());

            secondSectorExit = AND(arrSecondSector[andValueOneB], arrThirdSector[andTwoValueB]);
            secondSectorExit = XOR(arrSecondSector[feedForwardB], arrSecondSector[arrSecondSector.Length - 1], secondSectorExit.ToString());

            thirdSectorExit = AND(arrThirdSector[andValueOneC], arrThirdSector[andValueTwoC]);
            thirdSectorExit = XOR(arrThirdSector[feedForwardC], arrThirdSector[arrThirdSector.Length - 1], thirdSectorExit.ToString());


            //entries of each sector
            firsSectorEntry = XOR(arrFirstSector[feedBackA], thirdSectorExit.ToString());
            secondSectorEntry = XOR(firstSectorExit.ToString(), arrSecondSector[feedBackB]);
            thirdSectorEntry = XOR(secondSectorExit.ToString(), arrThirdSector[feedBackC]);

            PushBit(arrFirstSector, arrFirstSectorShadow, firsSectorEntry.ToString());
            PushBit(arrSecondSector, arrSecondSectorShadow, secondSectorEntry.ToString());
            PushBit(arrThirdSector, arrThirdSectorShadow, thirdSectorEntry.ToString());

            if (!isWarmUp)
            {
                exit = XOR(firstSectorExit.ToString(), secondSectorExit.ToString(), thirdSectorExit.ToString()).ToString();

            }
            return exit;
        }

        public Trivium Execute(string VI, string Key, string text, bool decript = false)
        {

            string[] message = new string[0];

            var exit = "";
            var exitBit = "";
            var count = 0;
            var encMessage = "";
            var model = new Trivium();
            var warm = true;

            arrFirstSector = InitSector(aValue, VI);
            arrSecondSector = InitSector(bValue, Key);
            arrThirdSector = InitSector(cValue, "", true);

            if (decript)
            {
                byteLength = 1;
                var arrEncoded = Base64Decode(text);
                text = GetBinaryFromBase64(arrEncoded);
                message = InitSector(text.Length * byteLength, text, false, true);
            }
            else
            {
                message = InitSector(text.Length * byteLength, text, false, false);
            }


            // war up 
            for (int i = 0; i < 1152; i++)
            {
                RunTrivium(warm, VI,Key);
                
            }

            // Geneterte encripted message.
            for (int i = 0; i < message.Length; i++)
            {
                exitBit = RunTrivium(false);
                if (exit == "")
                {
                    exit = XOR(exitBit, message[i].ToString()).ToString();
                }
                else
                {
                    if (count > 7)
                    {
                        exit += separator + XOR(exitBit, message[i].ToString()).ToString();
                        count = 0;
                    }
                    else
                    {
                        exit += XOR(exitBit, message[i].ToString()).ToString();
                    }
                }
                count++;

            }

            var encArr = exit.Split(',');
           

            if (decript)
            {
                encMessage = GetAssciValueFromBase64(encArr);
                model.Clear = encMessage;
            }
            else
            {
                encMessage = GenerateBase64FromBinary(encArr);
                model.Encrypted = encMessage;
            }

            return model;
        }

        private string AddParity(int length)
        {
            var result = "";

            for (int i = length; i < 8; i++)
            {
                result += valueZero;
            }

            return result;
        }

        private string GetBinaryFromBase64(byte[] result)
        {
            var binayText = "";

            foreach (var item in result)
            {
                var parity = GetBinary(item);

                if (parity.Length < 8)
                {

                    binayText += AddParity(parity.Length) + parity;
                }
                else
                {
                    binayText += parity;
                }

                
            }
            return binayText;
        }

        private string GenerateBase64FromBinary(string[] result)
        {
            var base64Text = "";
            var byteArr = new byte[result.Length];

            for (int i = 0; i < result.Length; i++)
            {
                var encLetter = result[i];
                var encAscii = GetDecimalFromBinary(encLetter);
                var byteItem = Convert.ToByte(encAscii);
                byteArr[i] = byteItem;

            }

            base64Text = Base64Encode(byteArr);
            return base64Text;
        }

        private string GetAssciValueFromBase64(string[] result)
        {
            var byteArr = new byte[result.Length];
           
            for (int i = 0; i < result.Length; i++)
            {
                var encLetter = result[i];
                var encAscii = GetDecimalFromBinary(encLetter);
               
                var byteItem = Convert.ToByte(encAscii);
                byteArr[i] = byteItem;

            }

            return Encoding.UTF8.GetString(byteArr);

        }

        private string[] PushBit(string[] arr, string[] arrShadow, string firsBit)
        {
            var exit = "";

            for (int c = 0; c < 1; c++)
            {
                var entrada = firsBit;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i == 0)
                    {
                        arrShadow[i] = entrada;
                        arrShadow[i + 1] = arr[i];
                    }
                    else if (i < arrShadow.Length - 1)
                    {
                        arrShadow[i + 1] = arr[i];
                    }
                    else
                    {
                        if (exit == "")
                        {
                            exit = arr[i].ToString();
                        }
                        else
                        {
                            exit += arr[i].ToString();
                        }
                    }

                }
                arrShadow.CopyTo(arr, 0);
            }

            return arrShadow;
        }

        private int XOR(string valueOne, string valueTwo, string ValueThree = "")
        {
            var result = 1;
            var chain = string.Concat(valueOne, valueTwo, ValueThree);
            var quantity = GetParity(chain, valueOfOne);
            if ((quantity % 2) == 0)
            {
                result = 0;
            }

            return result;
        }

        private int AND(string valueOne, string valueTwo)
        {
            var result = 0;
            if (valueOne == valueTwo && Convert.ToInt32(valueOne) > 0)
                result = 1;
            return result;
        }

        private string GetBinary(char value)
        {
            var result = "";
            var toConvert = Convert.ToByte(value);
            result = Convert.ToString(value, 2);
            result = AddParity(result.Length) + result;
            return result;
        }

        private string GetBinary(byte value)
        {
            var result = "";
            result = Convert.ToString(value, 2);
            return result;
        }

        private string GetAsciiFromBinary(string value)
        {
            var toConvert = GetDecimalFromBinary(value);
            var result = Convert.ToChar(toConvert).ToString();
            return result;
        }

        private int GetDecimalFromBinary(string value)
        {
            var result = Convert.ToInt32(value, 2);
            return result;
        }

        private string GetBinaryChain(string chain)
        {
            var arrChain = chain.ToCharArray();
            string exit = "";

            foreach (var item in arrChain)
            {
                if (exit == "")
                {
                    exit = GetBinary(item);
                }
                else
                {
                    exit += GetBinary(item);
                }
            }

            return exit;
        }

        private string[] InitSector(int size, string value = "", bool sectdef = false, bool binaryValues = false)
        {
            var binaryChain = "";
            if (!binaryValues)
            {
                binaryChain = GetBinaryChain(value);

            }
            else
            {
                if(value.Length < byteLength)
                {
                    value = AddParity(value.Length) + value;
                }
                
                binaryChain = value;
            }

            var arrBiKey = binaryChain.ToArray();
            string[] arr = new string[size];

            for (int i = 0; i < size; i++)
            {
                if (i < arrBiKey.Length)
                {
                    arr[i] = arrBiKey[i].ToString();
                }
                else
                {
                    if (sectdef && i > cSectorWarmUpFlag)
                    {
                        arr[i] = valueOfOne;
                    }
                    else
                    {
                        arr[i] = valueZero;
                    }

                }

            }

            return arr;

        }

        public int GetParity(string text, string keyToFind)
        {
            var result = 0;
            var arr = text.ToArray();
            int countKey = keyToFind.Length;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].ToString() == keyToFind)
                {
                    result++;
                }
            }


            return result;

        }

      

    }

    }