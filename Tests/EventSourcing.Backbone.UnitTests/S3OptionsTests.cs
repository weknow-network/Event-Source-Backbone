using Xunit;



namespace EventSourcing.Backbone
{
    public class S3OptionsTests

    {

        [Fact]
        public void S3Options_Equals_Test()
        {
            Assert.Equal(
                new S3Options { BasePath = "p/a/t/h", Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix },
                new S3Options { BasePath = "p/a/t/h", Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }
                );
            Assert.Equal(
                new S3Options { BasePath = "p/a/t/h" },
                new S3Options { BasePath = "p/a/t/h" }
                );
            Assert.Equal(
                new S3Options { Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix },
                new S3Options { Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }
                );
            Assert.Equal(
                new S3Options { EnvironmentConvention = S3EnvironmentConvention.BucketPrefix },
                new S3Options { EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }
                );
            Assert.Equal(
                new S3Options { Bucket = "root" },
                new S3Options { Bucket = "root" }
                );

            Assert.False(
                new S3Options { BasePath = "p/a/t/h", Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }.Equals(
                new S3Options { BasePath = "p/a/t/h", Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.None })
                );
            //var x = new S3Options { BasePath = "p/a/t/h", Bucket = "root" };
            //var y = new S3Options { BasePath = "p/a/t/h" };
            //Assert.False(x.Equals(y));
            //Assert.False(
            //    new S3Options { EnvironmentConvension = S3EnvironmentConvention.BucketPrefix }.Equals(
            //    new S3Options { Bucket = "root", EnvironmentConvension = S3EnvironmentConvention.BucketPrefix })
            //    );
        }

        [Fact]
        public void S3Options_GetHashCode_Test()
        {
            Assert.Equal(
                new S3Options { BasePath = "p/a/t/h", Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }.GetHashCode(),
                new S3Options { BasePath = "p/a/t/h", Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }.GetHashCode()
                );
            Assert.Equal(
                new S3Options { BasePath = "p/a/t/h" }.GetHashCode(),
                new S3Options { BasePath = "p/a/t/h" }.GetHashCode()
                );
            Assert.Equal(
                new S3Options { Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }.GetHashCode(),
                new S3Options { Bucket = "root", EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }.GetHashCode()
                );
            Assert.Equal(
                new S3Options { EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }.GetHashCode(),
                new S3Options { EnvironmentConvention = S3EnvironmentConvention.BucketPrefix }.GetHashCode()
                );
            Assert.Equal(
                new S3Options { Bucket = "root" }.GetHashCode(),
                new S3Options { Bucket = "root" }.GetHashCode()
                );

        }
    }
}
