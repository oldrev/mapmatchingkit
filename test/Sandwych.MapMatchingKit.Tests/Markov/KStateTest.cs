using Sandwych.MapMatchingKit.Markov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Sandwych.MapMatchingKit.Tests.Markov
{
    public class KStateTest
    {
        public struct MockStateTransition
        {
        }

        private class MockElem : AbstractStateCandidate<MockElem, MockStateTransition, MockSample>
        {
            private readonly int _id;

            public MockElem(int id, double seqprob, double filtprob, MockElem pred)
            {
                this._id = id;
                this.Seqprob = seqprob;
                this.Filtprob = filtprob;
                this.Predecessor = pred;
            }

            /*
            public MockElem(JSONObject json, MockFactory factory)
            {
                super(json, factory);
            }

            */
            public int Id => _id;

            public override bool Equals(MockElem other)
            {
                return this.Id == other.Id;
            }
        }

        /*
        private static class MockFactory : Factory<MockElem, MockStateTransition, Sample>
        {

            public MockElem candidate(JSONObject json)
            {
                return new MockElem(json, this);
            }

            public StateTransition transition(JSONObject json)
            {
                return new StateTransition(json);
            }

            public Sample sample(JSONObject json)
            {
                return new Sample(json);
            }
        }
        */

        [Fact]
        public void TestKStateUnbound()
        {
            var elements = new Dictionary<int, MockElem>();
            elements.Add(0, new MockElem(0, Math.Log10(0.3), 0.3, null));
            elements.Add(1, new MockElem(1, Math.Log10(0.2), 0.2, null));
            elements.Add(2, new MockElem(2, Math.Log10(0.5), 0.5, null));

            var state = new KState<MockElem, MockStateTransition, MockSample>();
            {
                var vector = new HashSet<MockElem>(new MockElem[] { elements[0], elements[1], elements[2] });

                state.Update(vector, new MockSample(0));

                Assert.Equal(3, state.Count);
                Assert.Equal(2, state.Estimate().Id);
            }

            elements.Add(3, new MockElem(3, Math.Log10(0.3), 0.3, elements[1]));
            elements.Add(4, new MockElem(4, Math.Log10(0.2), 0.2, elements[1]));
            elements.Add(5, new MockElem(5, Math.Log10(0.4), 0.4, elements[2]));
            elements.Add(6, new MockElem(6, Math.Log10(0.1), 0.1, elements[2]));

            {
                var vector = new HashSet<MockElem>(new MockElem[] {
                    elements[3], elements[4], elements[5], elements[6] });

                state.Update(vector, new MockSample(1));

                Assert.Equal(6, state.Count);
                Assert.Equal(5, state.Estimate().Id);

                var sequence = new int[] { 2, 5 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }

            elements.Add(7, new MockElem(7, Math.Log10(0.3), 0.3, elements[5]));
            elements.Add(8, new MockElem(8, Math.Log10(0.2), 0.2, elements[5]));
            elements.Add(9, new MockElem(9, Math.Log10(0.4), 0.4, elements[6]));
            elements.Add(10, new MockElem(10, Math.Log10(0.1), 0.1, elements[6]));

            {
                var vector = new HashSet<MockElem>(new MockElem[] {
                    elements[7], elements[8], elements[9], elements[10]
                });

                state.Update(vector, new MockSample(2));

                Assert.Equal(7, state.Count);
                Assert.Equal(9, state.Estimate().Id);

                var sequence = new int[] { 2, 6, 9 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }

            elements.Add(11, new MockElem(11, Math.Log10(0.3), 0.3, null));
            elements.Add(12, new MockElem(12, Math.Log10(0.2), 0.2, null));
            elements.Add(13, new MockElem(13, Math.Log10(0.4), 0.4, null));
            elements.Add(14, new MockElem(14, Math.Log10(0.1), 0.1, null));

            {
                var vector = new HashSet<MockElem>(new MockElem[] {
                    elements[11],
                    elements[12],
                    elements[13],
                    elements[14]
                });

                state.Update(vector, new MockSample(3));

                Assert.Equal(8, state.Count);
                Assert.Equal(13, state.Estimate().Id);

                var sequence = new int[] { 2, 6, 9, 13 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }
            {
                var vector = new HashSet<MockElem>();

                state.Update(vector, new MockSample(4));

                Assert.Equal(8, state.Count);
                Assert.Equal(13, state.Estimate().Id);

                var sequence = new int[] { 2, 6, 9, 13 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }
        }

        [Fact]
        public void TestBreak()
        {
            // Test k-state in case of HMM break 'no transition' as reported in barefoot issue #83.
            // Tests only 'no transitions', no emissions is empty vector and, hence, input to update
            // operation.

            var state = new KState<MockElem, MockStateTransition, MockSample>();
            var elements = new Dictionary<int, MockElem>();
            elements[0] = new MockElem(0, Math.Log10(0.4), 0.4, null);
            {
                var vector = new HashSet<MockElem>() { elements[0] };
                state.Update(vector, new MockSample(0));
            }
            elements[1] = new MockElem(1, Math.Log(0.7), 0.6, null);
            elements[2] = new MockElem(2, Math.Log(0.3), 0.4, elements[0]);
            {
                var vector = new HashSet<MockElem>() { elements[1], elements[2] };
                state.Update(vector, new MockSample(1));
            }
            elements[3] = new MockElem(3, Math.Log(0.5), 0.6, null);
            {
                var vector = new HashSet<MockElem> { elements[3] };
                state.Update(vector, new MockSample(2));
            }
            var seq = state.Sequence();
            Assert.Equal(0, seq.ElementAt(0).Id);
            Assert.Equal(1, seq.ElementAt(1).Id);
            Assert.Equal(3, seq.ElementAt(2).Id);
        }

        [Fact]
        public void TestKState()
        {
            var elements = new Dictionary<int, MockElem>();
            elements.Add(0, new MockElem(0, Math.Log10(0.3), 0.3, null));
            elements.Add(1, new MockElem(1, Math.Log10(0.2), 0.2, null));
            elements.Add(2, new MockElem(2, Math.Log10(0.5), 0.5, null));

            var state = new KState<MockElem, MockStateTransition, MockSample>(1, -1);
            {
                var vector = new HashSet<MockElem>() {
                    elements[0], elements[1], elements[2]
                };

                state.Update(vector, new MockSample(0));

                Assert.Equal(3, state.Count);
                Assert.Equal(2, state.Estimate().Id);
            }

            elements.Add(3, new MockElem(3, Math.Log10(0.3), 0.3, elements[1]));
            elements.Add(4, new MockElem(4, Math.Log10(0.2), 0.2, elements[1]));
            elements.Add(5, new MockElem(5, Math.Log10(0.4), 0.4, elements[2]));
            elements.Add(6, new MockElem(6, Math.Log10(0.1), 0.1, elements[2]));

            {
                var vector = new HashSet<MockElem>() {
                    elements[3], elements[4], elements[5], elements[6] };

                state.Update(vector, new MockSample(1));

                Assert.Equal(6, state.Count);
                Assert.Equal(5, state.Estimate().Id);

                var sequence = new int[] { 2, 5 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }

            elements.Add(7, new MockElem(7, Math.Log10(0.3), 0.3, elements[5]));
            elements.Add(8, new MockElem(8, Math.Log10(0.2), 0.2, elements[5]));
            elements.Add(9, new MockElem(9, Math.Log10(0.4), 0.4, elements[6]));
            elements.Add(10, new MockElem(10, Math.Log10(0.1), 0.1, elements[6]));

            {
                var vector = new HashSet<MockElem>() {
                    elements[7], elements[8], elements[9], elements[10] };

                state.Update(vector, new MockSample(2));

                Assert.Equal(6, state.Count);
                Assert.Equal(9, state.Estimate().Id);

                var sequence = new int[] { 6, 9 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }

            elements.Add(11, new MockElem(11, Math.Log10(0.3), 0.3, null));
            elements.Add(12, new MockElem(12, Math.Log10(0.2), 0.2, null));
            elements.Add(13, new MockElem(13, Math.Log10(0.4), 0.4, null));
            elements.Add(14, new MockElem(14, Math.Log10(0.1), 0.1, null));

            {
                var vector = new HashSet<MockElem>() {
                    elements[11], elements[12], elements[13], elements[14] };

                state.Update(vector, new MockSample(3));

                Assert.Equal(5, state.Count);
                Assert.Equal(13, state.Estimate().Id);

                var sequence = new int[] { 9, 13 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }
            {
                var vector = new HashSet<MockElem>();

                state.Update(vector, new MockSample(4));

                Assert.Equal(5, state.Count);
                Assert.Equal(13, state.Estimate().Id);

                var sequence = new int[] { 9, 13 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }
        }

        [Fact]
        public void TestTState()
        {
            var elements = new Dictionary<int, MockElem>();
            elements.Add(0, new MockElem(0, Math.Log10(0.3), 0.3, null));
            elements.Add(1, new MockElem(1, Math.Log10(0.2), 0.2, null));
            elements.Add(2, new MockElem(2, Math.Log10(0.5), 0.5, null));

            var state = new KState<MockElem, MockStateTransition, MockSample>(-1, 1);
            {
                var vector = new HashSet<MockElem>() { elements[0], elements[1], elements[2] };

                state.Update(vector, new MockSample(0));

                Assert.Equal(3, state.Count);
                Assert.Equal(2, state.Estimate().Id);
            }

            elements.Add(3, new MockElem(3, Math.Log10(0.3), 0.3, elements[1]));
            elements.Add(4, new MockElem(4, Math.Log10(0.2), 0.2, elements[1]));
            elements.Add(5, new MockElem(5, Math.Log10(0.4), 0.4, elements[2]));
            elements.Add(6, new MockElem(6, Math.Log10(0.1), 0.1, elements[2]));

            {
                var vector = new HashSet<MockElem>() {
                    elements[3], elements[4], elements[5], elements[6] };

                state.Update(vector, new MockSample(1));

                Assert.Equal(6, state.Count);
                Assert.Equal(5, state.Estimate().Id);

                var sequence = new int[] { 2, 5 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }

            elements.Add(7, new MockElem(7, Math.Log10(0.3), 0.3, elements[5]));
            elements.Add(8, new MockElem(8, Math.Log10(0.2), 0.2, elements[5]));
            elements.Add(9, new MockElem(9, Math.Log10(0.4), 0.4, elements[6]));
            elements.Add(10, new MockElem(10, Math.Log10(0.1), 0.1, elements[6]));

            {
                var vector = new HashSet<MockElem>() {
                    elements[7], elements[8], elements[9], elements[10] };

                state.Update(vector, new MockSample(2));

                Assert.Equal(6, state.Count);
                Assert.Equal(9, state.Estimate().Id);

                var sequence = new int[] { 6, 9 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }

            elements.Add(11, new MockElem(11, Math.Log10(0.3), 0.3, null));
            elements.Add(12, new MockElem(12, Math.Log10(0.2), 0.2, null));
            elements.Add(13, new MockElem(13, Math.Log10(0.4), 0.4, null));
            elements.Add(14, new MockElem(14, Math.Log10(0.1), 0.1, null));

            {
                var vector = new HashSet<MockElem>() {
                    elements[11], elements[12], elements[13], elements[14] };

                state.Update(vector, new MockSample(3));

                Assert.Equal(5, state.Count);
                Assert.Equal(13, state.Estimate().Id);

                var sequence = new int[] { 9, 13 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }
            {
                var vector = new HashSet<MockElem>();

                state.Update(vector, new MockSample(4));

                Assert.Equal(5, state.Count);
                Assert.Equal(13, state.Estimate().Id);

                var sequence = new int[] { 9, 13 };
                for (int i = 0; i < state.Sequence().Count() - 1; ++i)
                {
                    Assert.Equal(sequence[i], state.Sequence().ElementAt(i).Id);
                }
            }
        }

        /*
        public void TestKStateJSON()
        {
            Map<Integer, MockElem> elements = new HashMap<>();

            KState<MockElem, StateTransition, Sample> state = new KState<>(1, -1);

            {
                JSONObject json = state.toJSON();
                state = new KState<>(json, new MockFactory());
            }

            elements.put(0, new MockElem(0, Math.Log10(0.3), 0.3, null));
            elements.put(1, new MockElem(1, Math.Log10(0.2), 0.2, null));
            elements.put(2, new MockElem(2, Math.Log10(0.5), 0.5, null));

            state.update(
                    new HashSet<>(Arrays.asList(elements.get(0), elements.get(1), elements.get(2))),
                    new Sample(0));

            {
                JSONObject json = state.toJSON();
                state = new KState<>(json, new MockFactory());

                elements.clear();

                for (MockElem element : state.vector())
                {
                    elements.put(element.numid(), element);
                }
            }

            elements.put(3, new MockElem(3, Math.Log10(0.3), 0.3, elements.get(1)));
            elements.put(4, new MockElem(4, Math.Log10(0.2), 0.2, elements.get(1)));
            elements.put(5, new MockElem(5, Math.Log10(0.4), 0.4, elements.get(2)));
            elements.put(6, new MockElem(6, Math.Log10(0.1), 0.1, elements.get(2)));

            state.update(new HashSet<>(
                    Arrays.asList(elements.get(3), elements.get(4), elements.get(5), elements.get(6))),
                    new Sample(1));

            {
                JSONObject json = state.toJSON();
                state = new KState<>(json, new MockFactory());

                elements.clear();

                for (MockElem element : state.vector())
                {
                    elements.put(element.numid(), element);
                }
            }

            elements.put(7, new MockElem(7, Math.Log10(0.3), 0.3, elements.get(5)));
            elements.put(8, new MockElem(8, Math.Log10(0.2), 0.2, elements.get(5)));
            elements.put(9, new MockElem(9, Math.Log10(0.4), 0.4, elements.get(6)));
            elements.put(10, new MockElem(10, Math.Log10(0.1), 0.1, elements.get(6)));

            state.update(new HashSet<>(
                    Arrays.asList(elements.get(7), elements.get(8), elements.get(9), elements.get(10))),
                    new Sample(2));

            {
                JSONObject json = state.toJSON();
                state = new KState<>(json, new MockFactory());

                elements.clear();

                for (MockElem element : state.vector())
                {
                    elements.put(element.numid(), element);
                }
            }

            elements.put(11, new MockElem(11, Math.Log10(0.3), 0.3, null));
            elements.put(12, new MockElem(12, Math.Log10(0.2), 0.2, null));
            elements.put(13, new MockElem(13, Math.Log10(0.4), 0.4, null));
            elements.put(14, new MockElem(14, Math.Log10(0.1), 0.1, null));

            state.update(new HashSet<>(Arrays.asList(elements.get(11), elements.get(12),
                    elements.get(13), elements.get(14))), new Sample(3));

            state.update(new HashSet<MockElem>(), new Sample(4));

            {
                JSONObject json = state.toJSON();
                KState<MockElem, StateTransition, Sample> state2 =
                        new KState<>(json, new MockFactory());

                Assert.Equal(state.size(), state2.size());
                Assert.Equal(5, state2.size());
                Assert.Equal(state.estimate().numid(), state2.estimate().numid());
                Assert.Equal(13, state2.estimate().numid());
                Assert.Equal(state.sequence().size(), state2.sequence().size());
                Assert.Equal(2, state2.sequence().size());

                List<Integer> sequence = new LinkedList<>(Arrays.asList(9, 13));
                for (int i = 0; i < state.sequence().size() - 1; ++i)
                {
                    Assert.Equal(state.sequence().get(i).numid(), state2.sequence().get(i).numid());
                    Assert.Equal(sequence.get(i).longValue(), state2.sequence().get(i).numid());
                }
            }
        }
        */
    }
}
