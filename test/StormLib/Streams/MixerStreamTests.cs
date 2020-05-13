using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NUnit.Framework;
using StormLib.Streams;

namespace StormTests.StormLib.Streams
{
    public class MixerStreamTests
    {
        private static readonly string validMixerAccount = "https://mixer.com/xbox";

        [Test]
        public void Ctor_WhenUriNull_ThrowsArgNullEx()
        {
            Assert.Throws<ArgumentNullException>(() => new MixerStream(null));
        }

        [TestCase("http://mixer.com/xbox/")]
        [TestCase("https://mixer.com/xbox")]
        public void Ctor_NameSetCorrectly(string validAccount)
        {
            Uri uri = new Uri(validAccount);

            MixerStream ms = new MixerStream(uri);

            string expected = "xbox";
            string actual = ms.Name;

            foreach (string each in ms.Link.Segments)
            {
                Console.WriteLine(each);
            }

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ObjectsAreEqual_IEquatable()
        {
            MixerStream ms1 = new MixerStream(new Uri(validMixerAccount));
            MixerStream ms2 = new MixerStream(new Uri(validMixerAccount));

            bool actual = ms1.Equals(ms2);

            Assert.IsTrue(actual);
        }

        [Test]
        public void ObjectsAreEqual_DoubleEqualsSign()
        {
            MixerStream ms1 = new MixerStream(new Uri(validMixerAccount));
            MixerStream ms2 = new MixerStream(new Uri(validMixerAccount));

            bool actual = ms1 == ms2;

            Assert.IsTrue(actual);
        }

        [Test]
        public void ObjectsAreNotEqual_IEquatable()
        {
            MixerStream ms1 = new MixerStream(new Uri(validMixerAccount));
            MixerStream ms2 = new MixerStream(new Uri("https://mixer.com/fred"));

            bool actual = ms1.Equals(ms2);

            Assert.IsFalse(actual);
        }

        [Test]
        public void ObjectsAreNotEqual_DoubleEqualsSign()
        {
            MixerStream ms1 = new MixerStream(new Uri(validMixerAccount));
            MixerStream ms2 = new MixerStream(new Uri("https://mixer.com/fred"));

            bool actual = ms1 == ms2;

            Assert.IsFalse(actual);
        }
    }
}
