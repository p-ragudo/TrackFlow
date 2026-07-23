import React, { useState } from 'react';
import { Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import CreatableSelect from '../components/CreatableSelect';

interface AddPageProps {
    title: string
    onCancelPressed: () => void
    onSavePressed: () => void
    groups: string[]
    categories: string[]
    tags: string[]
}

export default function AddPage({ 
    title, 
    onCancelPressed, 
    onSavePressed, 
    groups, 
    categories, 
    tags 
}: AddPageProps) {
    const [form, setForm] = useState({
        name: '',
        group: '',
        category: '',
        tag: '',
        amount: '',
        description: ''
    })

    const handleChange = (field: string, value: string) => {
        setForm((prev) => ({...prev, [field]: value}))
    }

    return (
        <View style={styles.page}>
            <View style={styles.header}>
                <Text style={styles.headerText}>{title}</Text>
                <Pressable 
                    style={styles.cancelButton}
                    onPress={onCancelPressed}
                >Cancel</Pressable>
            </View>

            <View style={[styles.form, styles.formGap]}>
                <View>
                    <Text style={styles.inputLabel}>Name</Text>
                    <TextInput 
                        style={styles.input}
                        value={form.name}
                        onChangeText={(val) => handleChange('name', val)}
                    />
                </View>

                <View style={[styles.reverseContainer, styles.formGap]}>
                    <Pressable 
                        onPress={onSavePressed}
                        style={styles.saveButton}
                    >
                        Save
                    </Pressable>

                    <View>
                        <Text style={styles.inputLabel}>Description</Text>
                        <TextInput 
                            style={styles.input}
                            value={form.description}
                            onChangeText={(val) => handleChange('description', val)}
                        />
                    </View>

                    <View>
                        <Text style={styles.inputLabel}>Amount</Text>
                        <TextInput 
                            style={styles.input}
                            value={form.amount}
                            onChangeText={(val) => handleChange('amount', val)}
                        />
                    </View>

                    <CreatableSelect 
                        label="Tag"
                        value={form.tag}
                        onChangeText={(val: string) => handleChange('tag', val)}
                        options={tags}
                    />

                    <CreatableSelect 
                        label="Category"
                        value={form.category}
                        onChangeText={(val: string) => handleChange('category', val)}
                        options={categories}
                    />

                    <CreatableSelect 
                        label="Group"
                        value={form.group}
                        onChangeText={(val: string) => handleChange('group', val)}
                        options={groups}
                    />
                </View>
            </View>
        </View>
    )
}

const styles = StyleSheet.create({
    page: {
        padding: 20
    },
    header: {
        flexDirection: 'row',
        justifyContent: 'space-between'
    },
    form: {
        marginTop: 24
    },
    formGap: {
        gap: 20
    },
    headerText: {
        fontSize: 24,
        fontWeight: 500
    },
    cancelButton: {
        color: 'white',
        backgroundColor: 'black',
        paddingVertical: 8,
        paddingHorizontal: 16,
        borderRadius: 6 
    },
    input: {
        height: 44,
        borderWidth: 1,
        borderColor: '#8E8E8E',
        borderRadius: 8,
        paddingHorizontal: 12,
        fontSize: 15,
        color: '#000000',
        backgroundColor: '#FFFFFF',
    },
    inputLabel: {
        fontWeight: 600
    },
    reverseContainer: {
        flexDirection: 'column-reverse'
    },
    saveButton: {
        color: 'white',
        backgroundColor: 'black',
        alignItems: 'center',
        fontSize: 18,
        paddingVertical: 12,
        borderRadius: 8,
        marginTop: 20
    }
})