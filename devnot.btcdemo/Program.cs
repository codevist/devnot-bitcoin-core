using System;
using System.Text;
using System.Threading;
using NBitcoin;
using NBitcoin.Protocol;
using QBitNinja.Client;

namespace devnot.btcdemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var network = Network.TestNet;

            #region privateKey Address Relation 
            /*
            var privateKey = new Key();
            var bitcoinPrivateKey = privateKey.GetWif(network);
            var address = bitcoinPrivateKey.GetAddress();
*/

            #endregion

            //cW12usrBuvNTMrJqochW96HVuZsUebKd3GWVW3J2TxnjAGvw12Xc
            //cVh1vtGkdCYxveCpaW5yiBL7eSD3gXfZ6E7gBDZdpJYReBRFnj2Z

            var  bitcoinPrivateKey = new BitcoinSecret("cW12usrBuvNTMrJqochW96HVuZsUebKd3GWVW3J2TxnjAGvw12Xc", Network.TestNet);
            var addressReceiver = BitcoinAddress.Create("mvVaFrH6F5v9UHMghQGts53ozG6P1CSywn", Network.TestNet);


            Console.WriteLine("Private Anahtarın, kimseye gösterme: " + bitcoinPrivateKey);
            Console.WriteLine("Adresin: " + bitcoinPrivateKey.GetAddress());
            Console.WriteLine("Alıcının Adresi: " + addressReceiver);



            Console.WriteLine(bitcoinPrivateKey); 
            Console.WriteLine(bitcoinPrivateKey.GetAddress()); 



            var client = new QBitNinjaClient(network);
            var transactionId = uint256.Parse("fa8c13d7f66d907e94fe8cdccaa76eb7e67335bd4879bfe6d0be1ed4c8f9be08");
            var transactionResponse = client.GetTransaction(transactionId).Result;

            Console.WriteLine(transactionResponse.TransactionId); // 
            Console.WriteLine(transactionResponse.Block.Confirmations); // 


            var receivedCoins = transactionResponse.ReceivedCoins;
           
            OutPoint outPointToSpend = null;
            foreach (var coin in receivedCoins)
            {
                if (coin.TxOut.ScriptPubKey == bitcoinPrivateKey.ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }

            if (outPointToSpend == null)
                throw new Exception("Bu Coinler senin değil ");
            
            Console.WriteLine("{0} numaralı çıkışı harcayabilirsin. ", outPointToSpend.N + 1);

            //Giriş, harcanacak transaction
            var transaction = new Transaction();
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend
            });


            var minerFee = new Money(0.00007m, MoneyUnit.BTC);
            var receiverAmount = new Money(0.001m, MoneyUnit.BTC);


            // How much you want to get back as change
            var txInAmount = (Money)receivedCoins[(int)outPointToSpend.N].Amount;
            var changeAmount = txInAmount - receiverAmount - minerFee;


            TxOut receiverTxOut = new TxOut()
            {
                Value = receiverAmount,
                ScriptPubKey = addressReceiver.ScriptPubKey
            };

         
            TxOut changeBackTxOut = new TxOut()
            {
                Value = changeAmount,
                ScriptPubKey = bitcoinPrivateKey.ScriptPubKey
            };

            transaction.Outputs.Add(receiverTxOut);
            transaction.Outputs.Add(changeBackTxOut);



            transaction.Inputs[0].ScriptSig = bitcoinPrivateKey.ScriptPubKey;
            transaction.Sign(bitcoinPrivateKey, false);



            using (var node = Node.Connect(Network.TestNet,"185.28.76.179:18333")) //Connect to the node
            {
                node.VersionHandshake();                                        
                node.SendMessage(new InvPayload(InventoryType.MSG_TX, transaction.GetHash()));
                node.SendMessage(new TxPayload(transaction));
                Thread.Sleep(500);
            }

            Console.WriteLine("Coinler gitti. ");
        }
    }
}
